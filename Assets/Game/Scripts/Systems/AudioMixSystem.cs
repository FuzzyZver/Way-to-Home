using Leopotam.Ecs;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private const string MasterVolumeParam = "MasterVolume";
    private const string MusicVolumeParam = "MusicVolume";
    private const string SfxVolumeParam = "SfxVolume";
    private const string WorldVolumeParam = "WorldVolume";
    private const string WorldCutoffParam = "WorldCutoff";

    private AudioMixer _mixer;
    private PlayerActor _player;

    public void Init()
    {
        _mixer = GameConfig.SoundConfig.Mixer;
        _player = SceneData.Player;
    }

    public void Run()
    {
        if (_mixer == null) return;

        float worldVolume = 1f;
        float worldCutoff = HearingModifierComponent.OpenCutoff;

        var playerEntity = _player.GetEntity();
        if (playerEntity.Has<HearingModifierComponent>())
        {
            ref var hearingComp = ref playerEntity.Get<HearingModifierComponent>();
            worldVolume = hearingComp.WorldVolume;
            worldCutoff = hearingComp.WorldCutoff;
        }

        _mixer.SetFloat(MasterVolumeParam, LinearToDecibel(RealtimeData.MasterVolume));
        _mixer.SetFloat(MusicVolumeParam, LinearToDecibel(RealtimeData.MusicVolume));
        _mixer.SetFloat(SfxVolumeParam, LinearToDecibel(RealtimeData.SfxVolume));
        _mixer.SetFloat(WorldVolumeParam, LinearToDecibel(worldVolume));
        _mixer.SetFloat(WorldCutoffParam, Mathf.Clamp(worldCutoff, 10f, HearingModifierComponent.OpenCutoff));
    }

    private float LinearToDecibel(float linear)
    {
        return Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
    }
}