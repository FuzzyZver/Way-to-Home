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

    [Header("Flashlight dependence props")]
    public float FlashlightSampleInterval;    // 0.2
    public float FlashlightDependenceWindow;  // 60
    public float SweepMinAmplitude;           // 30
    public float SweepMaxAmplitude;           // 70
    public float SweepMinSpeed;               // 120
    public float SweepIdleSpeed;              // 20
    public float SweepStrokeBreak;            // 0.15
    public float SweepRateForMax;             // 20
    [Range(0f, 1f)] public float DependenceOnWeight;     // 0.5
    [Range(0f, 1f)] public float DependenceSweepWeight;  // 0.3
    [Range(0f, 1f)] public float DependenceFocusWeight;  // 0.2
}
