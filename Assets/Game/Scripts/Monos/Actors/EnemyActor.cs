using Leopotam.Ecs;
using UnityEngine;
using UnityEngine.AI;

public class EnemyActor: Actor
{
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private Transform _transform;
    [SerializeField] private Animator _animator;
    [SerializeField] private Renderer _renderer;
    public override void ExpandEntity(EcsEntity entity)
    {
        entity.Get<NavMeshAgentRef>().NavMeshAgent = _navMeshAgent;
        entity.Get<EnemyFolowFlag>();
        entity.Get<TransformRef>().Transform = _transform;
        entity.Get<AnimatorRef>().Animator = _animator;
        entity.Get<RendererRef>().Renderer = _renderer;
    }
}
