using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMetricsConfig", menuName = "Configs/PlayerMetricsConfig")]
public class PlayerMetricsConfig : ScriptableObject
{
    [Header("Light traching props")]
    public float RaycastCheckInterval;
    public float LightThreshold;
    public float LightIntensityNormality;
    public float SpotAngleMultiplier;
    public float LightPreferenceWindow;
    public LayerMask LightOcclusionMask;

    [Header("Look back traching props")]
    public float RotationFrequencyWindow;
    public float RotationAngleThreshold;
    public float RotationTimeThreshold;

    [Header("Fear freeze traching props")]
    public float FreezeFrequencyWindow;
    public float FreezeAngleThreshold;
    public float FreezeDistanceThreshold;
    public float FreezeTimeTrheshold;
    public float FearFreezeThreshold;

    [Header("Radial scan props")]
    public float RadialScanInterval;     // ~0.1
    public float RadialScanAngleStep;    // ~5
    public float RadialScanMaxDistance;  // ~40
    public float EyeHeight;              // ~1.7
    public LayerMask RadialScanMask;
}
