using UnityEngine;

[CreateAssetMenu(fileName = "ExecutionConfig", menuName = "Configs/ExecutionConfig")]
public class ExecutionConfig : ScriptableObject
{
    [Header("Light off settings")]
    public float LightOffDuration;
    public int MaxLightsPerCommand;

    [Header("Foot steps behind settings")]
    public float StepsOffset;
    public float SpeedMultiplier;
    public float RotationAngleThreshold;
    public float StepRotationTimeThreshold;
    public float FootstepsBehindDuration;
}
