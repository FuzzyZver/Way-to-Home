using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "SoundConfig", menuName = "Configs/SoundConfig")]
public class SoundConfig : ScriptableObject
{
    [Range(0f, 1f)] public float Volume;
    [Range(0f, 1f)] public float MusicVolume;
    [Range(0f, 1f)] public float SFXVolume;
    public List<AudioClip> CreepySounds;
    public List<AudioClip> LightHumSounds;
    public List<AudioClip> FlashlightFailSounds;
    public List<AudioClip> DeadSilenceSounds;

    [Header("Mixer")]
    public AudioMixer Mixer;
    public AudioMixerGroup WorldGroup;
    public AudioMixerGroup DirectorGroup;
    public int VoiceCount;   // 16
}
