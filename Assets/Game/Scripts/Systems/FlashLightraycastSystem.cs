using Leopotam.Ecs;
using UnityEngine;

public class FlashLightraycastSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private PlayerActor _playerRef;
    
    public void Init()
    {
        _playerRef = SceneData.Player;
    }

    public void Run()
    {
        var playerEntity = _playerRef.GetEntity();
        if (playerEntity.Has<DeadFlag>()) return;
        if (playerEntity.Has<FreezeFlag>()) return;

        var flashlight = playerEntity.Get<FlashlightComponent>().Light;
        if (Physics.Raycast(flashlight.transform.position, flashlight.transform.forward, out var hitInfo, flashlight.range))
        {
            EcsWorld.NewEntity().Get<FlashLightRaycastEvent>().HitInfo = hitInfo;
        }
    }
}
