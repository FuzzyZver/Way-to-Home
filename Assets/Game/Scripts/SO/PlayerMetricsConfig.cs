using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMetricsConfig", menuName = "Configs/PlayerMetricsConfig")]
public class PlayerMetricsConfig : ScriptableObject
{
    [Header("Light traching props")]
    public float RaycastCheckInterval;
    public float LightThreshold;
    public float LightIntensityNormality;
    public float SpotAngleMultiplier;
}
