using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundStepConfig", menuName = "Configs/SoundStepConfig", order = 0)]
public class SoundStepConfig : ScriptableObject
{
    public float DistanceForStep = 1.0f;

    [System.Serializable]
    public class StepSoundPack
    {
        public PhysicsMaterial material;
        public List<AudioClip> clips;
    }

    public List<StepSoundPack> soundPacks;

    private Dictionary<PhysicsMaterial, List<AudioClip>> _materialToClips;

    public void Inits()
    {
        _materialToClips = new Dictionary<PhysicsMaterial, List<AudioClip>>();
        foreach (var pack in soundPacks)
        {
            if (!_materialToClips.ContainsKey(pack.material))
                _materialToClips.Add(pack.material, pack.clips);
        }
    }

    public AudioClip GetRandomClip(PhysicsMaterial material)
    {
        if (_materialToClips == null) Inits();
        if (_materialToClips.TryGetValue(material, out var clips) && clips.Count > 0)
        {
            return clips[Random.Range(0, clips.Count)];
        }

        return null;
    }
}
