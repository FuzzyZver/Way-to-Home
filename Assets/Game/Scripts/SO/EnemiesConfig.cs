using UnityEngine;

[CreateAssetMenu(fileName = "EnemiesConfig", menuName = "Configs/EnemiesConfig")]
public class EnemiesConfig : ScriptableObject
{
    public EnemyActor EnemyActor;
    [Header("Spawn")]
    public float SpawnDistanceFromPlayer;
    public float SpawnCooldown;
    public int SpawbRaysCount;
    public float SpawnRaysAngel;
    [Header("SeekState")]

    [Header("HideState")]
    public float PlayerRaycastDistance;
    public float BehindOffsetDistance;
    public int FleeRaysCount;
    public int FleeRaysAngel;

    [Header("Fading")]
    public float FadeSpeed;
}
