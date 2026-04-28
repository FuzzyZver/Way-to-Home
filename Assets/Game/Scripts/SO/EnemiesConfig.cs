using UnityEngine;

[CreateAssetMenu(fileName = "EnemiesConfig", menuName = "Configs/EnemiesConfig")]
public class EnemiesConfig : ScriptableObject
{
    [Header("SeekState")]

    [Header("HideState")]
    public float PlayerRaycastDistance;
    public float BehindOffsetDistance;
    public int FleeRaysCount;
    public int FleeRaysAngel;

    [Header("Fading")]
    public float FadeSpeed;
}
