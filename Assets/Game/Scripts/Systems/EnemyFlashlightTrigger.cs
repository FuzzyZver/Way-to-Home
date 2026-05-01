using Leopotam.Ecs;
using UnityEngine;

public class EnemyFlashlightTrigger: Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<FlashLightRaycastEvent> _flashlightRaycastEventFilter;
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
                enemyNavMeshAgent.updateRotation = false;

                var enemyAnimator = enemyEntity.Get<AnimatorRef>().Animator;
                enemyAnimator.SetBool("IsWalking", false);
                enemyAnimator.SetBool("IsHiding", true);
            }
        }
    }
}
