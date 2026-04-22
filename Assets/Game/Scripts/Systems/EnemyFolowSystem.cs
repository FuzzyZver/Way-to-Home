using Leopotam.Ecs;
using UnityEngine;

public class EnemyFolowSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<EnemyFolowFlag> _enemyFolowFlagFilter;
    private PlayerActor _playerRef;

    public void Init()
    {
        _playerRef = SceneData.Player;
    }

    public void Run()
    {
        var playerEntity = _playerRef.GetEntity();
        if (playerEntity.Has<DeadFlag>()) return;
        if (playerEntity.Has<FreezeFlag>()) return;

        foreach (int i in _enemyFolowFlagFilter)
        {
            var enemyEntity = _enemyFolowFlagFilter.GetEntity(i);
            var enemyNavMeshAgent = enemyEntity.Get<NavMeshAgentRef>().NavMeshAgent;

            var playerTransform = playerEntity.Get<TransformRef>().Transform;
            if (Vector3.Distance(enemyNavMeshAgent.destination, playerTransform.position) > 5f)
            {
                enemyNavMeshAgent.SetDestination(playerTransform.position);
            }
        }
    }
}
