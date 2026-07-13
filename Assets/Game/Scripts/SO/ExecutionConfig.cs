using UnityEngine;

[CreateAssetMenu(fileName = "ExecutionConfig", menuName = "Configs/ExecutionConfig")]
public class ExecutionConfig : ScriptableObject
{
    [Header("Foot steps behind settings")]
    public float StepsOffset;
    public float SpeedMultiplier;
    public float RotationAngleThreshold;
    public float StepRotationTimeThreshold;
}
