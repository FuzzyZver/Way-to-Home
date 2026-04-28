using System.Collections.Generic;
using Leopotam.Ecs;
using UnityEngine;
using UnityEngine.AI;

public class InitSystem: Injects, IEcsPreInitSystem
{
    public void PreInit()
    {
        SceneData.Player.Init(EcsWorld);
        SceneData.Character.Init(EcsWorld);
        SceneData.Character.GetEntity().Get<CharacterComponent>() = new CharacterComponent
        {
            CharacterId = 1,
            CompleteDialogs = new List<bool>()
        };
        foreach(DialogueGroup dialogueGroup in GameConfig.TextConfig.FirstCharacterDialogs)
        {
            SceneData.Character.GetEntity().Get<CharacterComponent>().CompleteDialogs.Add(false);
        }

        //Render fog init
        RenderSettings.fog = true;
        RenderSettings.fogMode = GameConfig.CommonConfig.FogMode;
        RenderSettings.fogDensity = GameConfig.CommonConfig.FogDensity;
        RenderSettings.fogColor = GameConfig.CommonConfig.FogColor;
        Application.targetFrameRate = GameConfig.CommonConfig.FogFrameRate;
    }
}
