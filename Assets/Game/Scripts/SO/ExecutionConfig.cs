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
}
