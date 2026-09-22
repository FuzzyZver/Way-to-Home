using Leopotam.Ecs;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public class DrainFlashlightExecutorSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<DrainFlashlightEvent> _drainEventFilter;

    private PlayerActor _player;
    private Light _flashlight;

    private float _defaultDuration;
    private float _armTimeout;
    private float _soleLightDelay;
    private float _blackoutChance;
    private float _transitionDuration;
    private float _toggleMin;
    private float _toggleMax;
    private float _sputterLowMin;
    private float _sputterLowMax;
    private float _sputterHighMin;
    private List<AudioClip> _failSounds;

    public void Init()
    {
        _player = SceneData.Player;
        _flashlight = _player.GetEntity().Get<LightRef>().Light;

        var executionConfig = GameConfig.ExecutionConfig;
        _defaultDuration = executionConfig.DrainFlashlightDuration;
        _armTimeout = executionConfig.DrainArmTimeout;
        _soleLightDelay = executionConfig.DrainSoleLightDelay;
        _blackoutChance = executionConfig.DrainBlackoutChance;
        _transitionDuration = executionConfig.DrainTransitionDuration;
        _toggleMin = executionConfig.DrainToggleMin;
        _toggleMax = executionConfig.DrainToggleMax;
        _sputterLowMin = executionConfig.DrainSputterLowMin;
        _sputterLowMax = executionConfig.DrainSputterLowMax;
        _sputterHighMin = executionConfig.DrainSputterHighMin;
        _failSounds = GameConfig.SoundConfig.FlashlightFailSounds;
    }

    public void Run()
    {
        var playerEntity = _player.GetEntity();

        foreach (int i in _drainEventFilter)
        {
            ref var drainEvent = ref _drainEventFilter.Get1(i);
            Arm(playerEntity, in drainEvent);
        }

        if (!playerEntity.Has<FlashlightDrainComponent>()) return;
        if (playerEntity.Has<FreezeFlag>()) return;
        if (playerEntity.Has<DeadFlag>()) return;

        if (playerEntity.Has<FlashlightDischargedFlag>())
        {
            Cancel(playerEntity, "Flashlight discharged");
            return;
        }

        ref var drainComp = ref playerEntity.Get<FlashlightDrainComponent>();

        if (drainComp.Phase == DrainPhase.Armed)
        {
            TickArmed(playerEntity, ref drainComp);
            return;
        }

        if (!TickEffect(ref drainComp, out float multiplier))
        {
            Finish(playerEntity);
            return;
        }

        playerEntity.Get<FlashlightModifierComponent>().IntensityMultiplier = multiplier;
    }

    private void Arm(EcsEntity playerEntity, in DrainFlashlightEvent drainEvent)
    {
        if (playerEntity.Has<FlashlightDrainComponent>())
        {
            ReportSkip("Drain already in progress");
            return;
        }

        if (playerEntity.Has<FlashlightDischargedFlag>())
        {
            ReportSkip("Flashlight discharged");
            return;
        }

        playerEntity.Get<FlashlightDrainComponent>() = new FlashlightDrainComponent
        {
            Phase = DrainPhase.Armed,
            ArmRemaining = _armTimeout,
            SoleLightTime = 0f,
            Duration = drainEvent.Duration > 0f ? drainEvent.Duration : _defaultDuration
        };

        ReportArmed();
    }

    private void TickArmed(EcsEntity playerEntity, ref FlashlightDrainComponent drainComp)
    {
        drainComp.ArmRemaining -= Time.deltaTime;

        bool isSoleLight = _flashlight.enabled && !playerEntity.Get<PlayerLightMetrics>().IsCurrentlyInLight;
        drainComp.SoleLightTime = isSoleLight ? drainComp.SoleLightTime + Time.deltaTime : 0f;

        if (drainComp.SoleLightTime >= _soleLightDelay)
        {
            StartEffect(ref drainComp);
            return;
        }

        if (drainComp.ArmRemaining > 0f) return;

        Cancel(playerEntity, "No inconvenient moment found");
    }

    private void StartEffect(ref FlashlightDrainComponent drainComp)
    {
        drainComp.WithBlackout = Random.value < _blackoutChance;
        drainComp.Phase = DrainPhase.Sputter;

        drainComp.PhaseRemaining = drainComp.WithBlackout ? _transitionDuration : drainComp.Duration;

        drainComp.ToggleRemaining = 0f;
        drainComp.SputterLow = false;
        drainComp.SputterLevel = 1f;

        if (!drainComp.WithBlackout) PlayFailSound();
        ReportStart(drainComp.WithBlackout, drainComp.Duration);
    }

    private bool TickEffect(ref FlashlightDrainComponent drainComp, out float multiplier)
    {
        drainComp.PhaseRemaining -= Time.deltaTime;

        switch (drainComp.Phase)
        {
            case DrainPhase.Sputter:
                multiplier = TickSputter(ref drainComp);
                if (drainComp.PhaseRemaining > 0f) return true;
                if (!drainComp.WithBlackout) return false;

                drainComp.Phase = DrainPhase.Blackout;
                drainComp.PhaseRemaining = drainComp.Duration;
                PlayFailSound();
                return true;

            case DrainPhase.Blackout:
                multiplier = 0f;
                if (drainComp.PhaseRemaining > 0f) return true;

                drainComp.Phase = DrainPhase.Recover;
                drainComp.PhaseRemaining = _transitionDuration;
                return true;

            case DrainPhase.Recover:
                float progress = _transitionDuration > 0f
                    ? 1f - Mathf.Clamp01(drainComp.PhaseRemaining / _transitionDuration)
                    : 1f;
                multiplier = Mathf.Lerp(TickSputter(ref drainComp), 1f, progress);
                return drainComp.PhaseRemaining > 0f;

            default:
                multiplier = 1f;
                return false;
        }
    }

    private float TickSputter(ref FlashlightDrainComponent drainComp)
    {
        drainComp.ToggleRemaining -= Time.deltaTime;
        if (drainComp.ToggleRemaining > 0f) return drainComp.SputterLevel;

        drainComp.SputterLow = !drainComp.SputterLow;
        drainComp.ToggleRemaining = Random.Range(_toggleMin, _toggleMax);

        drainComp.SputterLevel = drainComp.SputterLow
            ? Random.Range(_sputterLowMin, _sputterLowMax)
            : Random.Range(_sputterHighMin, 1f);

        return drainComp.SputterLevel;
    }

    private void PlayFailSound()
    {
        if (_failSounds == null || _failSounds.Count == 0) return;

        EcsWorld.NewEntity().Get<AudioEffectEvent>() = new AudioEffectEvent
        {
            AudioClip = _failSounds[Random.Range(0, _failSounds.Count)],
            SoundPosition = _flashlight.transform.position
        };
    }

    private void Finish(EcsEntity playerEntity)
    {
        Cleanup(playerEntity);
        ReportRestored();
    }

    private void Cancel(EcsEntity playerEntity, string reason)
    {
        Cleanup(playerEntity);
        ReportCanceled(reason);
    }

    private void Cleanup(EcsEntity playerEntity)
    {
        if (playerEntity.Has<FlashlightModifierComponent>()) playerEntity.Del<FlashlightModifierComponent>();
        if (playerEntity.Has<FlashlightDrainComponent>()) playerEntity.Del<FlashlightDrainComponent>();
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportArmed()
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = "[DRAIN FLASHLIGHT] Armed, waiting for an inconvenient moment",
            Type = DebugType.Info
        };
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportStart(bool withBlackout, float duration)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = withBlackout
                ? $"[DRAIN FLASHLIGHT] Blackout for {duration:0.0}s"
                : $"[DRAIN FLASHLIGHT] Sputter for {duration:0.0}s",
            Type = DebugType.Info
        };
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportRestored()
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = "[DRAIN FLASHLIGHT] Restored",
            Type = DebugType.Info
        };
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportCanceled(string reason)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[DRAIN FLASHLIGHT] Canceled: {reason}",
            Type = DebugType.Warning
        };
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportSkip(string reason)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[DRAIN FLASHLIGHT] Skipped: {reason}",
            Type = DebugType.Warning
        };
    }
}