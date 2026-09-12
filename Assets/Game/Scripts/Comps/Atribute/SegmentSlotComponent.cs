using UnityEngine;

public struct SegmentSlotComponent
{
    public string SegmentId;
    public SegmentActor Actor;

    public SegmentRelativePosition Position;
    public float DistanceToPlayer;

    public float Familiarity;
    public float TimeSinceLastVisit;
    public float TimeSinceLastCommand;
}
