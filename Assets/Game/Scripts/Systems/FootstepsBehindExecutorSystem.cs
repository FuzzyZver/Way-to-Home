using UnityEngine;
using Leopotam.Ecs;
using System.Diagnostics;

public class FootstepsBehindExecutorSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<FootstepsBehindEvent> _footstepsBehindEventFilter;
    private EcsFilter<FootstepsBehindFlag, TransformRef> _footStepsBehindFlagFilter;

    private PlayerActor _player;
    private SoundStepConfig _stepSoundConfig;
    private float _speedMultiplier;
    private float _rotationSpeedMultiplier;
    private float _stepsOffset;
    private float _rotationAngleThreshold;
    private float _stepRotationTimeThreshold;
    private float _defaultDuration;

    public void Init()
    {
        _player = SceneData.Player;
        _stepSoundConfig = GameConfig.SoundStepConfig;
        _stepSoundConfig.Inits();
        _stepsOffset = GameConfig.ExecutionConfig.StepsOffset;
        _speedMultiplier = GameConfig.ExecutionConfig.SpeedMultiplier;
        _rotationSpeedMultiplier = _speedMultiplier * 15f;
        _rotationAngleThreshold = GameConfig.ExecutionConfig.RotationAngleThreshold;
        _stepRotationTimeThreshold = GameConfig.ExecutionConfig.StepRotationTimeThreshold;
        _defaultDuration = GameConfig.ExecutionConfig.FootstepsBehindDuration;
    }

    public void Run()
    {
        var playerEntity = _player.GetEntity();

        foreach (int i in _footstepsBehindEventFilter)
        {
            ref var footstepsBehindEvent = ref _footstepsBehindEventFilter.Get1(i);
            StartFootsteps(playerEntity, in footstepsBehindEvent);
        }

        if (playerEntity.Has<FreezeFlag>()) return;
        if (playerEntity.Has<DeadFlag>()) return;

        foreach (int i in _footStepsBehindFlagFilter)
        {
            ref var flagComp = ref _footStepsBehindFlagFilter.Get1(i);
            ref var transformRef = ref _footStepsBehindFlagFilter.Get2(i);
            var entity = _footStepsBehindFlagFilter.GetEntity(i);

            flagComp.Remaining -= Time.deltaTime;
            if (flagComp.Remaining <= 0f)
            {
                entity.Del<FootstepsBehindFlag>();
                ReportStop();
                continue;
            }

            ref var footstepsBehindComp = ref entity.Get<FootstepsBehindComponent>();

            bool isPlayerRotating = IsPlayerRotating(ref footstepsBehindComp, playerEntity.Get<CameraTargetRef>().Transform);

            if (playerEntity.Has<MoveFlag>())
            {
                footstepsBehindComp.PassedDistance += GameConfig.PlayerConfig.Speed * _speedMultiplier * Time.deltaTime;
            }
            else if (isPlayerRotating)
            {
                footstepsBehindComp.PassedDistance += GameConfig.PlayerConfig.Speed * _rotationSpeedMultiplier * Time.deltaTime;
            }

            if (footstepsBehindComp.PassedDistance >= _stepSoundConfig.DistanceForStep)
            {
                PlayFootstepSound(transformRef.Transform);
                footstepsBehindComp.PassedDistance = 0f;
            }
        }
    }

    private void StartFootsteps(EcsEntity playerEntity, in FootstepsBehindEvent footstepsBehindEvent)
    {
        float duration = footstepsBehindEvent.Duration > 0f ? footstepsBehindEvent.Duration : _defaultDuration;

        ref var flagComp = ref playerEntity.Get<FootstepsBehindFlag>();
        flagComp.Remaining = duration;

        ref var footstepsBehindComp = ref playerEntity.Get<FootstepsBehindComponent>();
        footstepsBehindComp.PassedDistance = 0f;
        footstepsBehindComp.Time = 0f;
        footstepsBehindComp.LastLookDirection = playerEntity.Get<CameraTargetRef>().Transform.forward;

        ReportStart(duration);
    }

    private void PlayFootstepSound(Transform transform)
    {
        var soundPosition = transform.position + transform.TransformDirection(Vector3.back * _stepsOffset);
        if (Physics.Raycast(soundPosition + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 3f))
        {
            var mat = hit.collider.sharedMaterial;
            if (mat != null)
            {
                var clip = _stepSoundConfig.GetRandomClip(mat);
                if (clip != null)
                {
                    EcsWorld.NewEntity().Get<AudioEffectEvent>() = new AudioEffectEvent
                    {
                        AudioClip = clip,
                        SoundPosition = soundPosition
                    };
                }
            }
        }
    }

    private bool IsPlayerRotating(ref FootstepsBehindComponent footstepsBehindComp, Transform cameraTransform)
    {
        bool isPlayerRotating = false;
        Vector3 lastLookDirection = footstepsBehindComp.LastLookDirection;
        float rotationAngle = Vector3.Angle(cameraTransform.forward, lastLookDirection);

        footstepsBehindComp.Time += Time.deltaTime;
        if (footstepsBehindComp.Time < _stepRotationTimeThreshold) return isPlayerRotating;
        if (rotationAngle > _rotationAngleThreshold) isPlayerRotating = true;
        footstepsBehindComp.LastLookDirection = cameraTransform.forward;
        footstepsBehindComp.Time = 0;
        return isPlayerRotating;
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportStart(float duration)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[FOOTSTEPS BEHIND] Started for {duration:0.0}s",
            Type = DebugType.Info
        };
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportStop()
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = "[FOOTSTEPS BEHIND] Stopped",
            Type = DebugType.Info
        };
    }
}