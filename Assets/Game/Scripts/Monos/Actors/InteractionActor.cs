using Leopotam.Ecs;
using UnityEngine;

public class InteractionActor: Actor
{
    [SerializeField] private Transform _transform;
    [SerializeField] private Transform _cameraTarget;

    public override void ExpandEntity(EcsEntity entity)
    {
        entity.Get<TransformRef>().Transform = _transform;
        entity.Get<CameraTargetRef>().Transform = _cameraTarget;
    }

    public void Interact()
    {
        var interactionEntity = GetEntity();
        if (interactionEntity.Has<CharacterComponent>())
        {
            GetWorld().NewEntity().Get<TextDialogEvent>().InteractionActor = this;
        }
    }
}
