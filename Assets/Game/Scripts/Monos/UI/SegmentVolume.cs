using UnityEngine;
using Leopotam.Ecs;

[RequireComponent(typeof(Collider))]
public class SegmentVolume : MonoBehaviour
{
    [SerializeField] private SegmentActor _segment;

    private void Reset()
    {
        _segment = GetComponentInParent<SegmentActor>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Emit(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        Emit(other, false);
    }

    private void Emit(Collider other, bool entered)
    {
        if (_segment == null) return;

        var world = _segment.GetWorld();
        if (world == null) return;

        world.NewEntity().Get<SegmentTriggerEvent>() = new SegmentTriggerEvent
        {
            Segment = _segment.GetEntity(),
            Other = other,
            Entered = entered
        };
    }
}