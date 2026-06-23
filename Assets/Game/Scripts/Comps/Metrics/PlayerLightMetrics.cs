using UnityEngine;

public struct PlayerLightMetrics
{
    public float TotalTimeInLight;
    public float TotalTimeInDark;
    public float LightPreferencesRatio;
    public float LightToDarkTransitions;
    public float DarkToLightTransitions;
    public float AverageLightLevel;
    public bool IsCurrentlyInLight;
}
