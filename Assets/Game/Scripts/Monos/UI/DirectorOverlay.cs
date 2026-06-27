using UnityEngine;
using System.Collections.Generic;
using Leopotam.Ecs;
using Input = UnityEngine.InputSystem.InputSystem;
using UnityEngine.InputSystem;

public sealed class DirectorOverlay : MonoBehaviour
{
#if DEV_OVERLAY
bool _show = false;
    private InputAction _overlayViewInput;
    [SerializeField] private string _overlayKeytag;

    GUIStyle _style;
    EcsEntity _playerModelEntity;
    EcsEntity _playerEntity;
    List<ToastView> _toasts;
    List<string> _activeCommands;
    int _removeRequest = -1;

    void Start()
    {
        _overlayViewInput = Input.actions.FindAction(_overlayKeytag);
        if (_overlayViewInput != null)
            _overlayViewInput.performed += OnOverlayViewInput;
        else
            Debug.LogError($"[DIRECTOR OVERLAY] Key tag |{_overlayKeytag}| for move is not recognized!" +
                           "Please check field in DirectorOverlay or Input System Settings!");
        _overlayViewInput.Enable();
    }

    void OnOverlayViewInput(InputAction.CallbackContext context)
    {
        _show = !_show;
    }

    void OnGUI()
    {
        if (!_show) return;
        _style ??= new GUIStyle(GUI.skin.label) { fontSize = 12, richText = true };

        DrawDirectorPanel();
        DrawToasts();
    }

    void DrawDirectorPanel()
    {
        GUILayout.BeginArea(new Rect(10, 10, 360, Screen.height - 20), GUI.skin.box);

        if (!_playerModelEntity.IsAlive())
        {
            GUILayout.Label("<no player entity>", _style);
        }
        else
        {
            ref var m = ref _playerModelEntity.Get<PlayerModel>();
            GUILayout.Label("<b>PLAYER METRICS</b>", _style);
            GUILayout.Space(4);
            GUILayout.Label($"Player Entity: {_playerEntity.Get<PlayerLightMetrics>().LightPreferencesRatio}", _style);
            GUILayout.Space(4);
            GUILayout.Label("<b>PLAYER MODEL</b>", _style);
            GUILayout.Space(4);
            GUILayout.Label($"Composure: {m.Composure}", _style);
        }

        GUILayout.Space(10);
        GUILayout.Label("<b>ACTIVE COMMANDS</b>", _style);
        if (_activeCommands == null || _activeCommands.Count == 0)
            GUILayout.Label("  <color=#888888>— none —</color>", _style);
        else
            for (int i = 0; i < _activeCommands.Count; i++)
                GUILayout.Label($"  * {_activeCommands[i]}", _style);

        GUILayout.EndArea();
    }

    void DrawToasts()
    {
        if (_toasts == null) return;

        const float w = 320f;
        GUILayout.BeginArea(new Rect(Screen.width - w - 10, 10, w, Screen.height - 20));

        for (int i = 0; i < _toasts.Count; i++)
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(_toasts[i].Text, _style);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", GUILayout.Width(22)))
                _removeRequest = i;
            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();
    }

    public void CaptureFrame(EcsEntity playerEntity,EcsEntity model, List<ToastView> toasts, List<string> activeCommands)
    {
        _playerEntity = playerEntity;
        _playerModelEntity = model;
        _toasts = toasts;
        _activeCommands = activeCommands;
    }

    public int ConsumeRemoveRequest()
    {
        int r = _removeRequest;
        _removeRequest = -1;
        return r;
    }

#endif
}