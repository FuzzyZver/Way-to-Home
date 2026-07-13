using UnityEngine;
using Leopotam.Ecs;

public class FootstepsBehindExecutorSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<FootstepsBehindFlag, TransformRef> _footStepsBehindFlagFilter;
    private PlayerActor _player;
    private SoundStepConfig _stepSoundConfig;
    private float _speedMultiplier;
    private float _rotationSpeedMultiplier;
    private float _stepsOffset;
    private float _rotationAngleThreshold;
    private float _stepRotationTimeThreshold;

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
    }

    public void Run()
    {
        var playerEntity = _player.GetEntity();
        if (playerEntity.Has<FreezeFlag>()) return;
        if (playerEntity.Has<DeadFlag>()) return;

        foreach (int i in _footStepsBehindFlagFilter)
        {
            ref var transformRef = ref _footStepsBehindFlagFilter.Get2(i);
            ref var footstepsBehindComp = ref _footStepsBehindFlagFilter.GetEntity(i).Get<FootstepsBehindComponent>();

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

    private void PlayFootstepSound(Transform transform)
    {
        var soundPosition = transform.position+ transform.TransformDirection(Vector3.back * _stepsOffset);
        if (Physics.Raycast(soundPosition + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 1.2f))
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
        if(footstepsBehindComp.Time < _stepRotationTimeThreshold) return isPlayerRotating;
        if (rotationAngle > _rotationAngleThreshold) isPlayerRotating = true;
        footstepsBehindComp.LastLookDirection = cameraTransform.forward;
        footstepsBehindComp.Time = 0;
        return isPlayerRotating;
    }
}
