using UnityEngine;
using System.Collections.Generic;

public struct SegmentSlotComponent
{
    public string SegmentId;
    public SegmentActor Actor;
    public List<SegmentObjects> Objects;

    public SegmentRelativePosition Position;
    public float DistanceToPlayer;

    public float Familiarity;
    public float TimeSinceLastVisit;
    public float TimeSinceLastCommand;
}
