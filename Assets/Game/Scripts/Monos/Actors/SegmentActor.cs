using System.Collections.Generic;
using UnityEngine;
using Leopotam.Ecs;

[System.Serializable]
public class SegmentAnchor
{
    public string Tag;
    public Transform Point;
}

public class SegmentActor : Actor
{
    [SerializeField] private string _segmentId;
    [SerializeField] private List<SegmentAnchor> _anchors = new List<SegmentAnchor>();

    public override void ExpandEntity(EcsEntity entity)
    {
        entity.Get<SegmentSlotComponent>() = new SegmentSlotComponent
        {
            SegmentId = string.IsNullOrEmpty(_segmentId) ? gameObject.name : _segmentId,
            Actor = this,
            Position = SegmentRelativePosition.Behind,
            Familiarity = 0f,
            TimeSinceLastVisit = float.MaxValue,
            TimeSinceLastCommand = float.MaxValue
        };
    }
}
