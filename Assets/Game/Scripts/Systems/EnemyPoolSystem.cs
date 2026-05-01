using Leopotam.Ecs;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class EnemyPoolSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<GetEnemyPoolEvent> _getEnemyPoolEventFilter;
    private EcsFilter<DespawnFlag> _despawnFlagFilter;
    public ObjectPool<EnemyActor> _enemyActorPool;
    public EnemiesConfig _enemyConfig;

    public void Init()
    {
        _enemyConfig = GameConfig.EnemiesConfig;
        _enemyActorPool = new ObjectPool<EnemyActor>(
            Create,
            OnEnemyTaken,
            OnEnemyRelease,
            OnDestroy,
            false,
            10,
            100
            );
    }

    public void Run()
    {
        foreach(int i in _getEnemyPoolEventFilter)
        {
            ref var eventComp = ref _getEnemyPoolEventFilter.Get1(i);
            Get(eventComp.Vector3);
        }

        foreach(int i in _despawnFlagFilter)
        {
            var enemyEntity = _despawnFlagFilter.GetEntity(i);
            Release(enemyEntity.Get<EnemyActorRef>().EnemyActor);
        }
    }

    private EnemyActor Create()
    {
        var enemy = GameObject.Instantiate(_enemyConfig.EnemyActor);
        enemy.Init(EcsWorld);
        return enemy;
    }

    private EnemyActor Get(Vector3 spawnPos)
    {
        var enemy = _enemyActorPool.Get();

        enemy.transform.position = spawnPos;

        var entity = enemy.GetEntity();

        if (!entity.IsAlive())
        {
            enemy.Init(EcsWorld);
            entity = enemy.GetEntity();
        }

        entity.Get<EnemyActorRef>().EnemyActor = enemy;

        return enemy;
    }

    private void OnEnemyTaken(EnemyActor enemy)
    {
        enemy.gameObject.SetActive(true);
    }

    private void Release(EnemyActor enemy)
    {
        _enemyActorPool.Release(enemy);
    }

    private void OnEnemyRelease(EnemyActor enemy)
    {
        if (enemy == null)
            Debug.LogError($"[ENEMY POOL SYSTEM] The enemy's entity is empty, and further work is disrupted!");
        if (enemy.gameObject == null)
            Debug.LogError($"[ENEMY POOL SYSTEM] The enemy's object is empty, and further work has been disrupted!");

        enemy.gameObject.SetActive(false);
    }

    private void OnDestroy(EnemyActor enemy)
    {
        enemy.GetEntity().Destroy();
    }
}
