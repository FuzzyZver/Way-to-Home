using Leopotam.Ecs;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFindHidePointSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<EnemyHideFlag> _enemyHideFlagFilter;
    private PlayerActor _playerRef;
    private EnemiesConfig _enemyConfig;

    public void Init()
    {
        _playerRef = SceneData.Player;
        _enemyConfig = GameConfig.EnemiesConfig;
    }

    public void Run()
    {
        var playerEntity = _playerRef.GetEntity();
        if (playerEntity.Has<DeadFlag>()) return;
        if (playerEntity.Has<FreezeFlag>()) return;

        foreach (int i in _enemyHideFlagFilter)
        {
            var enemyEntity = _enemyHideFlagFilter.GetEntity(i);
            var enemyNavMesh = enemyEntity.Get<NavMeshAgentRef>().NavMeshAgent;

            if (enemyEntity.Has<DespawnFlag>()) return;

            Vector3 fleePoint;
            bool found = TryFindBestFleePoint(
                playerEntity.Get<CameraRef>().Camera.transform,
                enemyEntity.Get<TransformRef>().Transform,
                enemyNavMesh,
                10000f,   // дальность луча
                2f,    // насколько "за объект"
                7,     // количество лучей
                100f,   // угол разброса
                out fleePoint
            );

            if (found)
            {
                if (!enemyEntity.Has<FadeComponent>())
                {
                    enemyEntity.Get<FadeComponent>() = new FadeComponent
                    {
                        Current = 1f,
                        Target = 0f,
                        Speed = 0.5f
                    };
                }

                enemyNavMesh.isStopped = false;
                if (Vector3.Distance(enemyNavMesh.destination, fleePoint) > 0.5f)
                {
                    enemyNavMesh.SetDestination(fleePoint);
                    Vector3 dirToPlayer = playerEntity.Get<CameraRef>().Camera.transform.position - enemyEntity.Get<TransformRef>().Transform.position;
                    dirToPlayer.y = 0;

                    if (dirToPlayer != Vector3.zero)
                    {
                        Quaternion lookRot = Quaternion.LookRotation(dirToPlayer);
                        enemyEntity.Get<TransformRef>().Transform.rotation = Quaternion.Slerp(
                            enemyEntity.Get<TransformRef>().Transform.rotation,
                            lookRot,
                            Time.deltaTime * 5f
                        );
                    }
                }
            }
        }
    }

    public static bool TryFindBestFleePoint(
        Transform playerCamera,
        Transform enemy,
        NavMeshAgent agent,
        float maxDistance,
        float offsetBehindObstacle,
        int raysCount,
        float angleSpread,
        out Vector3 bestPoint)
    {
        bestPoint = Vector3.zero;
        float bestScore = float.NegativeInfinity;

        Vector3 origin = playerCamera.position;

        int obstacleMask = 1 << LayerMask.NameToLayer("Obstacle");

        if (agent.remainingDistance < 1f)
        {
            Vector3 dirToEnemy = (enemy.position - playerCamera.position);
            if (Physics.Raycast(playerCamera.position, dirToEnemy.normalized, out RaycastHit hit, dirToEnemy.magnitude, obstacleMask))
            {
                return false;
            }
        }

        Vector3 forward = playerCamera.forward;

        for (int i = 0; i < raysCount; i++)
        {
            float t = (float)i / (raysCount - 1);
            float angle = Mathf.Lerp(-angleSpread, angleSpread, t);

            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance, obstacleMask))
            {
                float dynamicOffset = offsetBehindObstacle + agent.radius + 0.5f;
                Vector3 candidate = hit.point + dir * dynamicOffset;

                if (NavMesh.SamplePosition(candidate, out NavMeshHit navHit, 5f, NavMesh.AllAreas))
                {
                    Vector3 navPoint = navHit.position;

                    Vector3 dirToPoint = navPoint - origin;

                    bool isHidden = true;

                    if (Physics.Raycast(origin, dirToPoint.normalized, out RaycastHit hit2, dirToPoint.magnitude))
                    {
                        if (hit2.collider.transform == enemy)
                        {
                            isHidden = false;
                        }
                    }

                    if (!isHidden)
                        continue;

                    float distance = Vector3.Distance(navPoint, origin);

                    float score = distance;

                    if (Vector3.Distance(navPoint, agent.destination) < 2f)
                    {
                        score += 50f;
                    }

                    NavMeshPath path = new NavMeshPath();
                    if (agent.CalculatePath(navPoint, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        score += 100f;
                    }
                    else
                    {
                        continue;
                    }

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestPoint = navPoint;
                    }
                }
            }
        }

        return bestScore > float.NegativeInfinity;
    }
}
