using Leopotam.Ecs;
using UnityEngine;

public class ObjectActivatesSystem: Injects, IEcsRunSystem
{
    private EcsFilter<ActivatesFlag> _activatesFlagFilter;
    
    public void Run()
    {
        foreach(int i in _activatesFlagFilter)
        {
            ref var flagComp = ref _activatesFlagFilter.Get1(i);

            if (flagComp.Duration > 0)
            {
                flagComp.Duration -= 0.1f;
                continue;
            }
            else
            {
                flagComp.GameObject.SetActive(true);
                _activatesFlagFilter.GetEntity(i).Del<ActivatesFlag>();
            }
            
        }
    }
}
