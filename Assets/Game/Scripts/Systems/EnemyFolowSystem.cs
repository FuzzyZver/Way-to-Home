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
        bool paused = playerEntity.Has<DeadFlag>() || playerEntity.Has<FreezeFlag>();

        foreach (int i in _enemyFolowFlagFilter)
        {
            var enemyEntity = _enemyFolowFlagFilter.GetEntity(i);
            var enemyNavMeshAgent = enemyEntity.Get<NavMeshAgentRef>().NavMeshAgent;
            var enemyAnimator = enemyEntity.Get<AnimatorRef>().Animator;

            if (paused)
            {
                enemyNavMeshAgent.isStopped = true;
                enemyNavMeshAgent.updateRotation = false;
                enemyAnimator.SetBool("IsWalking", false);
                continue;
            }

            enemyNavMeshAgent.isStopped = false;
            enemyNavMeshAgent.updateRotation = true;

            var playerTransform = playerEntity.Get<TransformRef>().Transform;

            if (Vector3.Distance(enemyNavMeshAgent.destination, playerTransform.position) > 5f)
            {
                enemyNavMeshAgent.SetDestination(playerTransform.position);
            }

            enemyAnimator.SetBool("IsWalking", true);
        }
    }
}
