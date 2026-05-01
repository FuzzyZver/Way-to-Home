using Leopotam.Ecs;
using UnityEngine;

public class CameraFocusSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<CameraFocusFlag> _cameraFocusFlagFilter;
    private PlayerActor _playerRef;
    
    public void Init()
    {
        _playerRef = SceneData.Player;
    }

    public void Run()
    {
        var playerEntity = _playerRef.GetEntity();
        if (playerEntity.Has<DeadFlag>()) return;

        foreach(int i in _cameraFocusFlagFilter)
        {
            ref var flagComp = ref _cameraFocusFlagFilter.Get1(i);
            var cameraTarget = playerEntity.Get<CameraTargetRef>().Transform;
            cameraTarget.transform.LookAt(flagComp.Transform);
        }
    }
}
