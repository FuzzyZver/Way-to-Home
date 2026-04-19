using Leopotam.Ecs;
using UnityEngine;

public class PlayerActor: Actor
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Transform _transform;
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _cameraTargetTransform;

    public override void ExpandEntity(EcsEntity entity)
    {
        entity.Get<RigidbodyRef>().Rigidbody = _rigidbody;
        entity.Get<TransformRef>().Transform = _transform;
        entity.Get<CameraRef>().Camera = _camera;
        entity.Get<CameraTargetRef>().Transform = _cameraTargetTransform;
    }
}
