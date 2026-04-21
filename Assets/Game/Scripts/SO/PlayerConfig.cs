using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    public float Speed;

    [Header("Flashlight")]
    public float ScrollSpeed;
    public float FlashlightMaxRange;
    public float FlashlightMinRange;
    public float SpotAngel;
    public float Intensity;
}
