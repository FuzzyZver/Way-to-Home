using Leopotam.Ecs;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LookSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<LookInputEvent> _lookInputEventFilter;
    private InputConfig _inputConfig;
    private Vector2 _velocity;
    private Vector2 _frameVelocity;

    private PlayerActor _playerRef;
    private Transform _playerTransform;
    
    public void Init()
    {
        _inputConfig = GameConfig.InputConfig;
        _playerRef = SceneData.Player;
        _playerTransform = _playerRef.GetEntity().Get<TransformRef>().Transform;
    }

    public void Run()
    {
        var playerEntity = _playerRef.GetEntity();
        if (playerEntity.Has<DeadFlag>()) return;
        if (playerEntity.Has<FreezeFlag>()) return;

        foreach (int i in _lookInputEventFilter)
        {
            ref var eventComp = ref _lookInputEventFilter.Get1(i);

            Vector2 mouseDelta = new Vector2(eventComp.Vector2.x, eventComp.Vector2.y);
            Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * _inputConfig.LookSensitivity);
            _frameVelocity = Vector2.Lerp(_frameVelocity, rawFrameVelocity, 1 / _inputConfig.LookSmoothing);
            _velocity += _frameVelocity;
            _velocity.y = Mathf.Clamp(_velocity.y, -_inputConfig.LookVerticalLimit, _inputConfig.LookVerticalLimit);

            var cameraTarget = playerEntity.Get<CameraTargetRef>().Transform;

            cameraTarget.localRotation = Quaternion.Euler(-_velocity.y, 0f, 0f);
            var cameraTransform = playerEntity.Get<CameraRef>().Camera.transform;
            cameraTransform.localRotation = Quaternion.AngleAxis(-_velocity.y, Vector3.right);
            _playerTransform.localRotation = Quaternion.AngleAxis(_velocity.x, Vector3.up);
        }
    }
}
