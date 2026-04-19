using Leopotam.Ecs;
using UnityEngine;

public class InitSystem: Injects, IEcsPreInitSystem
{
    public void PreInit()
    {
        SceneData.Player.Init(EcsWorld);
    }
}
