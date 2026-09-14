using UnityEngine;

public struct BlinkingLightFlag
{
    public float Duration;

    public bool Initialized;
    public float BaseIntensity;
    public float NoiseSeed;

    public bool InBurst;
    public float StateRemaining;
    public float ToggleRemaining;
    public bool BurstOn;
}
