using System.Collections.Generic;
using UnityEngine;
using Leopotam.Ecs;

[System.Serializable]
public class SegmentObjects
{
    public SegmentObjectsType Type;
    public GameObject Object;
}

public class SegmentActor : Actor
{
    [SerializeField] private string _segmentId;
    [SerializeField] private List<SegmentObjects> _objects = new List<SegmentObjects>();

    public override void ExpandEntity(EcsEntity entity)
    {
        entity.Get<SegmentSlotComponent>() = new SegmentSlotComponent
        {
            SegmentId = string.IsNullOrEmpty(_segmentId) ? gameObject.name : _segmentId,
            Actor = this,
            Objects = _objects,
            Position = SegmentRelativePosition.Behind,
            Familiarity = 0f,
            TimeSinceLastVisit = float.MaxValue,
            TimeSinceLastCommand = float.MaxValue
        };
    }
}
