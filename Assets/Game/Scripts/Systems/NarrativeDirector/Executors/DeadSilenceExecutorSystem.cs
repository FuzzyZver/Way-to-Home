using Leopotam.Ecs;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public class DeadSilenceExecutorSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<DeadSilenceEvent> _silenceEventFilter;

    private PlayerActor _player;

    private float _defaultDuration;
    private float _cutDuration;
    private float _restoreDuration;
    private float _floor;
    private float _closedCutoff;
    private float _stingerVolume;
    private float _stingerDistance;
    private float _stingerTail;
    private List<AudioClip> _stingerClips;

    public void Init()
    {
        _player = SceneData.Player;

        var executionConfig = GameConfig.ExecutionConfig;
        _defaultDuration = executionConfig.DeadSilenceDuration;
        _cutDuration = executionConfig.DeadSilenceCutDuration;
        _restoreDuration = executionConfig.DeadSilenceRestoreDuration;
        _floor = executionConfig.DeadSilenceFloor;
        _closedCutoff = executionConfig.DeadSilenceCutoff;
        _stingerVolume = executionConfig.DeadSilenceStingerVolume;
        _stingerDistance = executionConfig.DeadSilenceStingerDistance;
        _stingerTail = executionConfig.DeadSilenceStingerTail;
        _stingerClips = GameConfig.SoundConfig.DeadSilenceSounds;
    }

    public void Run()
    {
        var playerEntity = _player.GetEntity();

        foreach (int i in _silenceEventFilter)
        {
            ref var silenceEvent = ref _silenceEventFilter.Get1(i);
            StartSilence(playerEntity, in silenceEvent);
        }

        if (!playerEntity.Has<DeadSilenceComponent>()) return;

        if (playerEntity.Has<DeadFlag>())
        {
            Finish(playerEntity);
            return;
        }

        if (playerEntity.Has<FreezeFlag>()) return;

        ref var silenceComp = ref playerEntity.Get<DeadSilenceComponent>();
        ref var hearingComp = ref playerEntity.Get<HearingModifierComponent>();

        silenceComp.PhaseRemaining -= Time.deltaTime;
        float progress = silenceComp.PhaseDuration > 0f
            ? 1f - Mathf.Clamp01(silenceComp.PhaseRemaining / silenceComp.PhaseDuration)
            : 1f;

        switch (silenceComp.Phase)
        {
            case SilencePhase.Cut:
                ApplyHearing(ref hearingComp, progress);
                if (silenceComp.PhaseRemaining > 0f) return;
                EnterPhase(ref silenceComp, SilencePhase.Hold, silenceComp.HoldDuration);
                return;

            case SilencePhase.Hold:
                ApplyHearing(ref hearingComp, 1f);
                if (silenceComp.PhaseRemaining > 0f) return;
                EnterStinger(playerEntity, ref silenceComp);
                return;

            case SilencePhase.Stinger:
                ApplyHearing(ref hearingComp, 1f);
                if (silenceComp.PhaseRemaining > 0f) return;
                EnterPhase(ref silenceComp, SilencePhase.Restore, _restoreDuration);
                return;

            case SilencePhase.Restore:
                ApplyHearing(ref hearingComp, 1f - progress);
                if (silenceComp.PhaseRemaining > 0f) return;
                Finish(playerEntity);
                return;
        }
    }

    private void StartSilence(EcsEntity playerEntity, in DeadSilenceEvent silenceEvent)
    {
        if (playerEntity.Has<DeadSilenceComponent>())
        {
            ReportSkip("Silence already in progress");
            return;
        }

        float holdDuration = silenceEvent.Duration > 0f ? silenceEvent.Duration : _defaultDuration;

        playerEntity.Get<DeadSilenceComponent>() = new DeadSilenceComponent
        {
            Phase = SilencePhase.Cut,
            PhaseDuration = _cutDuration,
            PhaseRemaining = _cutDuration,
            HoldDuration = holdDuration
        };

        playerEntity.Get<HearingModifierComponent>() = new HearingModifierComponent
        {
            WorldVolume = 1f,
            WorldCutoff = HearingModifierComponent.OpenCutoff
        };

        ReportStart(holdDuration);
    }

    private void EnterStinger(EcsEntity playerEntity, ref DeadSilenceComponent silenceComp)
    {
        var clip = PickStinger();
        if (clip == null)
        {
            EnterPhase(ref silenceComp, SilencePhase.Restore, _restoreDuration);
            return;
        }

        var playerTransform = playerEntity.Get<TransformRef>().Transform;

        Vector3 back = -playerTransform.forward;
        back.y = 0f;
        if (back.sqrMagnitude < 0.001f) back = Vector3.back;
        back.Normalize();

        Vector3 side = Vector3.Cross(Vector3.up, back) * Random.Range(-0.6f, 0.6f);
        Vector3 position = playerTransform.position
                         + (back + side).normalized * _stingerDistance
                         + Vector3.up * 1.5f;

        EcsWorld.NewEntity().Get<AudioEffectEvent>() = new AudioEffectEvent
        {
            AudioClip = clip,
            SoundPosition = position,
            Channel = AudioChannel.Director,
            Volume = _stingerVolume
        };

        EnterPhase(ref silenceComp, SilencePhase.Stinger, clip.length + _stingerTail);
        ReportStinger(clip.name);
    }

    private void ApplyHearing(ref HearingModifierComponent hearingComp, float depth)
    {
        depth = Mathf.Clamp01(depth);

        float fade = depth * (2f - depth);

        hearingComp.WorldVolume = Mathf.Lerp(1f, _floor, fade);

        float openLog = Mathf.Log(HearingModifierComponent.OpenCutoff);
        float closedLog = Mathf.Log(Mathf.Max(10f, _closedCutoff));
        hearingComp.WorldCutoff = Mathf.Exp(Mathf.Lerp(openLog, closedLog, fade));
    }

    private void EnterPhase(ref DeadSilenceComponent silenceComp, SilencePhase phase, float duration)
    {
        silenceComp.Phase = phase;
        silenceComp.PhaseDuration = duration;
        silenceComp.PhaseRemaining = duration;
    }

    private AudioClip PickStinger()
    {
        if (_stingerClips == null || _stingerClips.Count == 0) return null;
        return _stingerClips[Random.Range(0, _stingerClips.Count)];
    }

    private void Finish(EcsEntity playerEntity)
    {
        if (playerEntity.Has<HearingModifierComponent>()) playerEntity.Del<HearingModifierComponent>();
        if (playerEntity.Has<DeadSilenceComponent>()) playerEntity.Del<DeadSilenceComponent>();
        ReportRestored();
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportStart(float holdDuration)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[DEAD SILENCE] Started, hold {holdDuration:0.0}s",
            Type = DebugType.Info
        };
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportStinger(string clipName)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[DEAD SILENCE] Wrong sound: {clipName}",
            Type = DebugType.Info
        };
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportRestored()
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = "[DEAD SILENCE] Hearing restored",
            Type = DebugType.Info
        };
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportSkip(string reason)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[DEAD SILENCE] Skipped: {reason}",
            Type = DebugType.Warning
        };
    }
}