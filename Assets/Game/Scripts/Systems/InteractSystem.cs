using Leopotam.Ecs;
using UnityEngine;

public class InteractSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<InteractInputEvent> _interactInputEventFilter;
    private InputConfig _inputConfig;
    private PlayerActor _playerRef;

    public void Init()
    {
        _inputConfig = GameConfig.InputConfig;
        _playerRef = SceneData.Player;
    }

    public void Run()
    {
        var playerEntity = _playerRef.GetEntity();
        if (playerEntity.Has<DeadFlag>()) return;
        if (playerEntity.Has<FreezeFlag>()) return;

        foreach (int i in _interactInputEventFilter)
        {
            var cameraTransform = playerEntity.Get<CameraRef>().Camera.transform;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hitInfo, _inputConfig.InteractionDistance))
            {
                if (hitInfo.collider.TryGetComponent<InteractionActor>(out InteractionActor interactionActor))
                {
                    interactionActor.Interact();
                }
            }
        }
    }    
}
