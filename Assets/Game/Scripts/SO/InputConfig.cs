using UnityEngine;

[CreateAssetMenu(fileName = "InputConfig", menuName = "Configs/InputConfig")]
public class InputConfig : ScriptableObject
{
    public string MoveKeyTag;
    public string JumpKeyTag;
    public string InteractionKeyTag;
    public string LookKeyTag;

    [Header("Other props")]
    public float MoveInputGravity;
    public float LookSensitivity;
    public float LookSmoothing;
    public float LookVerticalLimit;
}
