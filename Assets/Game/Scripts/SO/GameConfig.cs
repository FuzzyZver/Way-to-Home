using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/GameConfig")]
public class GameConfig : ScriptableObject
{
    public InputConfig InputConfig;
    public PlayerConfig PlayerConfig;
    public EnemiesConfig EnemiesConfig;
    public TextConfig TextConfig;
    public CommonConfig CommonConfig;
}
