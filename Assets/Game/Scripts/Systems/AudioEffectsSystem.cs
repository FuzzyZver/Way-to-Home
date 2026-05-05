using Leopotam.Ecs;
using UnityEngine;

public class AudioEffectsSystem: Injects, IEcsRunSystem
{
    private EcsFilter<AudioEffectEvent> _audioEffectEventFilter;
    private float _volume;

    public void Run()
    {
        _volume = GameConfig.SoundConfig.Volume;

        foreach (int audioEventIndex in _audioEffectEventFilter)
        {
            AudioClip audioClip = _audioEffectEventFilter.Get1(audioEventIndex).AudioClip;
            Vector3 soundPosition = _audioEffectEventFilter.Get1(audioEventIndex).SoundPosition;
            AudioSource.PlayClipAtPoint(audioClip, soundPosition, _volume);
        }
    }
}
