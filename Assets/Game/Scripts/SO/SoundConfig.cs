using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SoundConfig", menuName = "Configs/SoundConfig")]
public class SoundConfig : ScriptableObject
{
    [Range(0f, 1f)] public float Volume;
    [Range(0f, 1f)] public float MusicVolume;
    [Range(0f, 1f)] public float SFXVolume;
    public List<AudioClip> CreepySounds;
}
