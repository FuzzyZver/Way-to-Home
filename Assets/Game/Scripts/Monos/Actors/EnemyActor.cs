using Leopotam.Ecs;
using UnityEngine;
using UnityEngine.AI;

public class EnemyActor: Actor
{
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private Transform _transform;
    public override void ExpandEntity(EcsEntity entity)
    {
        entity.Get<NavMeshAgentRef>().NavMeshAgent = _navMeshAgent;
        entity.Get<EnemyFolowFlag>();
        entity.Get<TransformRef>().Transform = _transform;
    }
}
