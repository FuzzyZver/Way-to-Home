using Leopotam.Ecs;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class EnemiesSpawnSystem:Injects, IEcsInitSystem, IEcsRunSystem
{
    private EnemiesConfig _enemiesConfig;
    private PlayerActor _playerRef;
    private float _lastSpawnTime;
    private NavMeshSurface _navmeshSurface;

    public void Init()
    {
        _lastSpawnTime = Time.time;
        _enemiesConfig = GameConfig.EnemiesConfig;
        _playerRef = SceneData.Player;
        _navmeshSurface = SceneData.NavMeshSurface;
    }

    public void Run()
    {
        var playerEntity = _playerRef.GetEntity();
        if (playerEntity.Has<DeadFlag>()) return;
        if (playerEntity.Has<FreezeFlag>()) return;

        if (Time.time - _lastSpawnTime < _enemiesConfig.SpawnCooldown) return;
        _lastSpawnTime = Time.time;

        var playerTransform = playerEntity.Get<TransformRef>().Transform;
        TryFindBestSpawnPoint(
            playerTransform,
            _enemiesConfig.EnemyActor.GetComponent<NavMeshAgent>(),
            _enemiesConfig.SpawnDistanceFromPlayer,
            _enemiesConfig.BehindOffsetDistance,
            _enemiesConfig.SpawbRaysCount,
            _enemiesConfig.SpawnRaysAngel,
            out Vector3 spawnPoint
            );
        var enemyActor = GameObject.Instantiate(_enemiesConfig.EnemyActor, spawnPoint, Quaternion.identity);
        enemyActor.Init(EcsWorld);

    }

    public static bool TryFindBestSpawnPoint(
        Transform playerTransform,
        NavMeshAgent agent,
        float maxDistance,
        float offsetBehindObstacle,
        int raysCount,
        float angleSpread,
        out Vector3 spawnPoint)
    {
        spawnPoint = Vector3.zero;
        float bestScore = float.NegativeInfinity;

        Vector3 origin = playerTransform.position;

        int obstacleMask = 1 << LayerMask.NameToLayer("Obstacle");

        Vector3 forward = playerTransform.forward;

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
                    if (navHit.position.y > playerTransform.position.y + 1f) continue;

                    Vector3 dirToPoint = navPoint - origin;

                    float distance = Vector3.Distance(navPoint, origin);

                    float score = distance;

                    if (Vector3.Distance(navPoint, playerTransform.position) > 5f)
                    {
                        score += 50f;
                    }

                    if (score > bestScore)
                    {
                        bestScore = score;
                        spawnPoint = navPoint;
                    }
                }
            }
        }

        return bestScore > float.NegativeInfinity;
    }
}
