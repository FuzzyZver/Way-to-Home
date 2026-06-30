using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMetricsConfig", menuName = "Configs/PlayerMetricsConfig")]
public class PlayerMetricsConfig : ScriptableObject
{
    [Header("Light traching props")]
    public float RaycastCheckInterval;
    public float LightThreshold;
    public float LightIntensityNormality;
    public float SpotAngleMultiplier;

    [Header("Look back traching props")]
    public float RotationFrequencyWindow;
    public float RotationAngleThreshold;
    public float RotationTimeThreshold;
}
