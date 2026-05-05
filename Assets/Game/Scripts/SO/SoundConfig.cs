using UnityEngine;

[CreateAssetMenu(fileName = "SoundConfig", menuName = "Configs/SoundConfig")]
public class SoundConfig : ScriptableObject
{
    [Range(0f, 1f)] public float Volume;
}
