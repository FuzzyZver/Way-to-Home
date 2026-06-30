using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SoundConfig", menuName = "Configs/SoundConfig")]
public class SoundConfig : ScriptableObject
{
    [Range(0f, 1f)] public float Volume;
    public List<AudioClip> CreepySounds;
}
