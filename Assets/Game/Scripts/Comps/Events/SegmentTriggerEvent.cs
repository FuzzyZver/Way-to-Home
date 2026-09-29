using UnityEngine;
using Leopotam.Ecs;

public struct SegmentTriggerEvent
{
    public EcsEntity Segment;
    public Collider Other;
    public bool Entered;
}