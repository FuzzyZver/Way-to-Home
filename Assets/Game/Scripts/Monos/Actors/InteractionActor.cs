using Leopotam.Ecs;
using UnityEngine;

public class InteractionActor: Actor
{
    [SerializeField] private Transform _transform;

    public override void ExpandEntity(EcsEntity entity)
    {
        entity.Get<TransformRef>().Transform = _transform;
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
