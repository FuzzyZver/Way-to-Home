using Input = UnityEngine.InputSystem.InputSystem;
using UnityEngine.InputSystem;
using UnityEngine;
using Leopotam.Ecs;

public class InputSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private InputAction _moveInputAction;
    private InputAction _jumpInputAction;
    private InputAction _interacionInputAction;
    private InputAction _lookInputAction;

    public void Init()
    {
        string moveKeyTag = GameConfig.InputConfig.MoveKeyTag;
        _moveInputAction = Input.actions.FindAction(moveKeyTag);
        if (_moveInputAction == null)
            Debug.LogError($"[INPUT SYSTEM] Key tag |{moveKeyTag}| for move is not recognized!" +
                           "Please check Input Config or Input System Settings!");

        string jumpKeytag = GameConfig.InputConfig.JumpKeyTag;
        _jumpInputAction = Input.actions.FindAction(jumpKeytag);
        if (_jumpInputAction != null)
            _jumpInputAction.performed += OnJunpKeyPress;
        else
            Debug.LogError($"[INPUT SYSTEM] Key tag |{jumpKeytag}| for jump is not recognized!" +
                               "Please check Input Config or Input System Settings!");

        string interactionKeytag = GameConfig.InputConfig.InteractionKeyTag;
        _interacionInputAction = Input.actions.FindAction(interactionKeytag);
        if (_interacionInputAction != null)
            _interacionInputAction.performed += OnInteractionKeyPress;
        else
            Debug.LogError($"[INPUT SYSTEM] Key tag |{interactionKeytag}| for interaction is not recognized!" +
                               "Please check Input Config or Input System Settings!");

        string lookKeyTag = GameConfig.InputConfig.LookKeyTag;
        _lookInputAction = Input.actions.FindAction(lookKeyTag);
        if (_lookInputAction == null)
            Debug.LogError($"[INPUT SYSTEM] Key tag |{lookKeyTag}| for look is not recognized!" +
                           "Please check Input Config or Input System Settings!");

        _moveInputAction.Enable();
        _jumpInputAction.Enable();
        _interacionInputAction.Enable();
        _lookInputAction.Enable();
    }

    private void OnJunpKeyPress(InputAction.CallbackContext callbackContext)
    {
        //EcsWorld.NewEntity().Get<JumpInputEvent>();
    }

    private void OnInteractionKeyPress(InputAction.CallbackContext callbackContext)
    {
        //EcsWorld.NewEntity().Get<InteractInputEvent>();
    }

    public void Run()
    {
        var moveInputValue = _moveInputAction.ReadValue<Vector2>();
        EcsWorld.NewEntity().Get<MoveInputEvent>().Vector2 = moveInputValue;

        var lookInputValue = _lookInputAction.ReadValue<Vector2>();
        EcsWorld.NewEntity().Get<LookInputEvent>().Vector2 = lookInputValue;
    }
}