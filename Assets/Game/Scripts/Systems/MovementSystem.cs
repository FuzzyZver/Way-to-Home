using UnityEngine;
using Leopotam.Ecs;

public class MovementSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<MoveInputEvent> _moveInputEventFilter;
    private PlayerConfig _playerConfig;
    private InputConfig _inputConfig;
    private Vector3 _previousInput;
    private PlayerActor _playerRef;

    public void Init()
    {
        _playerConfig = GameConfig.PlayerConfig;
        _inputConfig = GameConfig.InputConfig;
        _playerRef = SceneData.Player;
    }

    public void Run()
    {
        var playerEntity = _playerRef.GetEntity();
        if (playerEntity.Has<DeadFlag>()) return;
        if (playerEntity.Has<FreezeFlag>()) return;

        foreach (int i in _moveInputEventFilter)
        {
            ref var eventComp = ref _moveInputEventFilter.Get1(i);
            float movingSpeed = _playerConfig.Speed; 
            Vector2 lerpedInput = Vector2.Lerp(_previousInput, eventComp.Vector2, _inputConfig.MoveInputGravity * Time.deltaTime);
            _previousInput = lerpedInput;

            Vector2 targetVelocity = new Vector2(lerpedInput.x * movingSpeed, lerpedInput.y * movingSpeed);
            
            var playerRigidbody = playerEntity.Get<RigidbodyRef>().Rigidbody;
            var targetRbVelocity = new Vector3(targetVelocity.x, playerRigidbody.linearVelocity.y, targetVelocity.y);
            playerRigidbody.linearVelocity = playerRigidbody.transform.rotation * targetRbVelocity;

            if (targetVelocity.x > 0)
            {
                playerEntity.Get<MoveFlag>();
            }
            else
            {
                playerEntity.Del<MoveFlag>();
            }
        }
    }
}