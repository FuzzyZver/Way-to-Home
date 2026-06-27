using UnityEngine;
using Leopotam.Ecs;
using System.Collections.Generic;

#if DEV_OVERLAY
public sealed class DirectorOverlayBridgeSystem: Injects, IEcsRunSystem
{
    private EcsFilter<DebugEvent> _debugEventFilter;
    private EcsFilter<Command, CommandActiveFlag> _commandActiveFlagFilter;
    private EcsFilter<PlayerModel> _playerModelFilter;
    private EcsEntity _playerModelEntity;

    private readonly List<ToastView> _toasts = new List<ToastView>();
    private readonly List<string> _activeCommands = new List<string>();
    private const float ToastLife = 5f;
    private const int MaxToasts = 8;

    public void Run()
    {
        int rm = UI.DirectorOverlay.ConsumeRemoveRequest();
        if (rm >= 0 && rm < _toasts.Count)
            _toasts.RemoveAt(rm);

        foreach (int i in _debugEventFilter)
        {
            ref var debugComp = ref _debugEventFilter.Get1(i);
            if (_toasts.Count >= MaxToasts) _toasts.RemoveAt(0);
            _toasts.Add(new ToastView
            {
                Text = debugComp.Message,
                Birth = Time.unscaledTime
            });
        }

        float now = Time.unscaledTime;
        for (int i = _toasts.Count - 1; i >= 0; i--)
            if (now - _toasts[i].Birth >= ToastLife)
                _toasts.RemoveAt(i);

        foreach (int i in _commandActiveFlagFilter)
        {
            _activeCommands.Clear();
            ref var cmd = ref _commandActiveFlagFilter.Get1(i);
            _activeCommands.Add(cmd.Name);
        }

        foreach (int i in _playerModelFilter)
        {
            _playerModelEntity = _playerModelFilter.GetEntity(i);
        }
        UI.DirectorOverlay.CaptureFrame(SceneData.Player.GetEntity(), _playerModelEntity, _toasts, _activeCommands);
    }
}

public struct ToastView
{
    public string Text;
    public float Birth;
}
#endif