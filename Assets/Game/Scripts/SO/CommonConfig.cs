using UnityEngine;

[CreateAssetMenu(fileName = "CommonConfig", menuName = "Configs/CommonConfig")]
public class CommonConfig : ScriptableObject
{
    [Header("FogProps")]
    public float FogDensity;
    public int FogFrameRate;
    public Color FogColor;
    public FogMode FogMode;
}
