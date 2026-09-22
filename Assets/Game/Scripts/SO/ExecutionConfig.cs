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

    [Header("Drain flashlight settings")]
    public float DrainFlashlightDuration;              // 3
    public float DrainArmTimeout;                      // 25
    public float DrainSoleLightDelay;                  // 1.5
    [Range(0f, 1f)] public float DrainBlackoutChance;  // 0.4
    public float DrainTransitionDuration;              // 0.5
    public float DrainToggleMin;                       // 0.03
    public float DrainToggleMax;                       // 0.18
    [Range(0f, 1f)] public float DrainSputterLowMin;   // 0
    [Range(0f, 1f)] public float DrainSputterLowMax;   // 0.35
    [Range(0f, 1f)] public float DrainSputterHighMin;  // 0.6

    [Header("Dead silence settings")]
    public float DeadSilenceDuration;                       // 6
    public float DeadSilenceCutDuration;                    // 0.3 
    public float DeadSilenceRestoreDuration;                // 2.5 
    [Range(0f, 1f)] public float DeadSilenceFloor;          // 0.04
    public float DeadSilenceCutoff;                         // 350 Гц
    [Range(0f, 1f)] public float DeadSilenceStingerVolume;  // 0.35
    public float DeadSilenceStingerDistance;                // 3
    public float DeadSilenceStingerTail;                    // 0.8
}
