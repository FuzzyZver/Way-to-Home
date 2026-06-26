using UnityEngine;
using Leopotam.Ecs;

#if DEV_OVERLAY
public sealed class DirectorOverlayBridgeSystem: Injects, IEcsRunSystem
{
    private EcsFilter<DebugEvent> _debugEventFilter;
    private EcsFilter<Command, CommandActiveFlag> _commandActiveFlagFilter;
    private EcsFilter<PlayerModel> _playerModelFilter;
    private EcsEntity _playerModelEntity;

    public void Run()
    {
        foreach(int i in _debugEventFilter)
        {

        }

        foreach(int i in _commandActiveFlagFilter)
        {

        }

        foreach (int i in _playerModelFilter)
        {
            _playerModelEntity = _playerModelFilter.GetEntity(i);
        }
        UI.DirectorOverlay.CaptureFrame(SceneData.Player.GetEntity(), _playerModelEntity);
    }
}
#endif
