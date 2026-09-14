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

    [Header("Stalker glimpse settings")]
    public StalkerShadowActor ShadowActor;
    public float StalkerGlimpseDuration;
    public int StalkerGlimpseSlots;
    public float ShadowMinSpawnDistance;
    public float ShadowMaxSpawnDistance;
    public float ShadowSpawnHiddenCone;      // полный угол, ~140
    public float ShadowGazeCone;             // полный угол, ~35
    public float ShadowGazeDwellTime;        // ~0.15
    public float ShadowVanishSpeedOnGaze;    // высокая, ~8
    public float ShadowVanishSpeedOnTimeout; // низкая, ~1
    public float ShadowChestHeight;          // ~1.2

    [Header("Blinking light settings")]
    public float BlinkCalmMin;        // ~1.5
    public float BlinkCalmMax;        // ~4
    public float BlinkBurstMin;       // ~0.2
    public float BlinkBurstMax;       // ~0.8
    public float BlinkToggleMin;      // ~0.02
    public float BlinkToggleMax;      // ~0.12
    public float BlinkCalmJitter;     // ~0.12 — глубина дыхания в покое
    public float BlinkCalmNoiseSpeed; // ~2
    public float BlinkBurstPeak;      // ~1.3 — пересвет, обязательно выше 1
    public float BlinkBurstFloor;     // ~0

    [Header("Make light hostile settings")]
    public GameObject ShadowCasterPrefab;
    public float MakeLightHostileDuration;
    public float HostileShadowLateralOffset;                      // ~1.5
    [Range(0.05f, 0.95f)] public float HostileShadowCasterPlacement; // ~0.6
    [Range(0f, 1f)] public float HostileHumVolume;
}
