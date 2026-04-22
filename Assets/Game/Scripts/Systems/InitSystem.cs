using Leopotam.Ecs;
using UnityEngine;
using UnityEngine.AI;

public class InitSystem: Injects, IEcsPreInitSystem
{
    public void PreInit()
    {
        SceneData.Player.Init(EcsWorld);
        
        foreach(EnemyActor enemyActor in SceneData.Enemies)
        {
            enemyActor.Init(EcsWorld);
        }

        //Render fog init
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.07f;
        RenderSettings.fogColor = Color.darkGray;
        Application.targetFrameRate = 30;
    }
}
