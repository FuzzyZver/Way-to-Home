using Leopotam.Ecs;
using UnityEngine;

public class InitSystem: Injects, IEcsPreInitSystem
{
    public void PreInit()
    {
        SceneData.Player.Init(EcsWorld);

        //Render fog init
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.2f;
        RenderSettings.fogColor = Color.gray;
        Application.targetFrameRate = 30;
    }
}
