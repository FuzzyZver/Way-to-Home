using UnityEngine;

public struct RadialScanSample
{
    public float Angle;
    public float Distance;
    public Vector3 Point;
    public Collider Collider;
}

public struct RadialScanEvent
{
    public RadialScanSample[] Samples;
    public int Count;
    public float AngleStep;
    public float MaxDistance;
    public Vector3 Origin;
    public Vector3 Forward;
}