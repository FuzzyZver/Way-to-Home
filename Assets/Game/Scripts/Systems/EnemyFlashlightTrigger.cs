using Leopotam.Ecs;
using UnityEngine;

public class EnemyFlashlightTrigger: Injects, IEcsRunSystem
{
    private EcsFilter<FlashLightRaycastEvent> _flashlightRaycastEventFilter;

    public void Run()
    {
        foreach (int i in _flashlightRaycastEventFilter)
        {
            ref var eventComp = ref _flashlightRaycastEventFilter.Get1(i);
            if (eventComp.HitInfo.collider.gameObject.TryGetComponent(out EnemyActor enemy))
            {
                var enemyEntity = enemy.GetEntity();
                enemyEntity.Del<EnemyFolowFlag>();
                enemyEntity.Get<EnemyHideFlag>();
                var enemyNavMeshAgent = enemyEntity.Get<NavMeshAgentRef>().NavMeshAgent;
                enemyNavMeshAgent.isStopped = true;
                enemyNavMeshAgent.ResetPath();
            }
        }
    }
}
