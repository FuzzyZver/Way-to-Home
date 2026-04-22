using Leopotam.Ecs;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFindHidePointSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<EnemyHideFlag> _enemyHideFlagFilter;
    private PlayerActor _playerRef;
    private EnemiesConfig _enemyConfig;
    private float _maxDistance = 100f;
    private float[] angles = { -60f, -45f, -30f, - 15f, 0f, 15f, 30f, 45f, 60f };

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

            Vector3 fleePoint;
            bool found = TryFindBestFleePoint(
                playerEntity.Get<CameraRef>().Camera.transform,
                enemyEntity.Get<TransformRef>().Transform,
                enemyNavMesh,
                1000f,   // дальность луча
                2f,    // насколько "за объект"
                7,     // количество лучей
                60f,   // угол разброса
                out fleePoint
            );

            if (found)
            {
                enemyNavMesh.isStopped = false;
                if (Vector3.Distance(enemyNavMesh.destination, fleePoint) > 0.5f)
                {
                    enemyNavMesh.SetDestination(fleePoint);
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

        // центр взгляда игрока
        Vector3 forward = playerCamera.forward;

        for (int i = 0; i < raysCount; i++)
        {
            // равномерное распределение лучей
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
