using Leopotam.Ecs;
using UnityEngine;
using UnityEngine.Audio;

public class AudioEffectsSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private const int DefaultVoiceCount = 16;

    private EcsFilter<AudioEffectEvent> _audioEffectEventFilter;

    private AudioSource[] _voices;
    private int _nextVoice;
    private AudioMixerGroup _worldGroup;
    private AudioMixerGroup _directorGroup;

    public void Init()
    {
        var soundConfig = GameConfig.SoundConfig;
        _worldGroup = soundConfig.WorldGroup;
        _directorGroup = soundConfig.DirectorGroup;

        int voiceCount = soundConfig.VoiceCount > 0 ? soundConfig.VoiceCount : DefaultVoiceCount;
        _voices = new AudioSource[voiceCount];

        var root = new GameObject("AudioVoices");
        for (int i = 0; i < voiceCount; i++)
        {
            var voiceObject = new GameObject($"Voice_{i}");
            voiceObject.transform.SetParent(root.transform);

            var source = voiceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;

            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = 1f;
            source.maxDistance = 500f;

            _voices[i] = source;
        }
    }

    public void Run()
    {
        foreach (int i in _audioEffectEventFilter)
        {
            ref var audioEvent = ref _audioEffectEventFilter.Get1(i);
            if (audioEvent.AudioClip == null) continue;

            var voice = TakeVoice();
            voice.transform.position = audioEvent.SoundPosition;
            voice.outputAudioMixerGroup = audioEvent.Channel == AudioChannel.Director ? _directorGroup : _worldGroup;
            voice.clip = audioEvent.AudioClip;

            voice.volume = audioEvent.Volume > 0f ? audioEvent.Volume : 1f;
            voice.Play();
        }
    }

    private AudioSource TakeVoice()
    {
        for (int step = 0; step < _voices.Length; step++)
        {
            int index = (_nextVoice + step) % _voices.Length;
            if (_voices[index].isPlaying) continue;

            _nextVoice = (index + 1) % _voices.Length;
            return _voices[index];
        }

        var stolen = _voices[_nextVoice];
        _nextVoice = (_nextVoice + 1) % _voices.Length;
        return stolen;
    }
}