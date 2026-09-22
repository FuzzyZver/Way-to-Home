using System.Collections.Generic;
using Leopotam.Ecs;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.EventSystems.EventTrigger;

public class InitSystem: Injects, IEcsPreInitSystem
{
    public void PreInit()
    {
        SceneData.Player.Init(EcsWorld);
        foreach(var character in SceneData.Interactions)
        {
            character.Init(EcsWorld);
        }
        foreach(var segment in SceneData.Segments)
        {
            segment.Init(EcsWorld);
        }

        RealtimeData.MasterVolume = GameConfig.SoundConfig.Volume;
        RealtimeData.MusicVolume = GameConfig.SoundConfig.MusicVolume;
        RealtimeData.SfxVolume = GameConfig.SoundConfig.SFXVolume;

        //Render fog init
        RenderSettings.fog = true;
        RenderSettings.fogMode = GameConfig.CommonConfig.FogMode;
        RenderSettings.fogDensity = GameConfig.CommonConfig.FogDensity;
        RenderSettings.fogColor = GameConfig.CommonConfig.FogColor;
        Application.targetFrameRate = GameConfig.CommonConfig.FogFrameRate;
    }
}
