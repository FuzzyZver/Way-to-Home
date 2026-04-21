using Leopotam.Ecs;
using UnityEngine;

public class PlayerActor: Actor
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Transform _transform;
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _cameraTargetTransform;
    [SerializeField] private Light _flashlightLight;

    public override void ExpandEntity(EcsEntity entity)
    {
        entity.Get<RigidbodyRef>().Rigidbody = _rigidbody;
        entity.Get<TransformRef>().Transform = _transform;
        entity.Get<CameraRef>().Camera = _camera;
        entity.Get<CameraTargetRef>().Transform = _cameraTargetTransform;
        entity.Get<FlashlightComponent>().Light = _flashlightLight;
    }
}
