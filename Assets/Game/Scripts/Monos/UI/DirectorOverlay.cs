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

        GUILayout.BeginArea(new Rect(10, 10, 360, Screen.height - 20), GUI.skin.box);
        ref var modelComp = ref _playerModelEntity.Get<PlayerModel>();
        if (!_playerModelEntity.IsAlive() || !_playerEntity.IsAlive())
        {
            GUILayout.Label("<no player entity>", _style);
            GUILayout.EndArea();
            return;
        }
        GUILayout.Label($"Composure: {modelComp.Composure:F3}", _style);
        GUILayout.Space(10);
        GUILayout.Label($"Player Entity: {_playerEntity.Get<PlayerLightMetrics>().LightPreferencesRatio}", _style);
        GUILayout.EndArea();
    }

    public void CaptureFrame(EcsEntity playerEntity, EcsEntity modelEntity)
    {
        _playerEntity = playerEntity;
        _playerModelEntity = modelEntity;
    }

#endif
}
