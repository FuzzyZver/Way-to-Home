using UnityEngine;
using Leopotam.Ecs;
using System.Diagnostics;

public class DistanceToPlayerSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<SegmentSlotComponent> _slotsFilter;

    private PlayerActor _player;

#if DEV_OVERLAY
    private string _lastReportedSlotId;
#endif

    public void Init()
    {
        _player = SceneData.Player;
        ReportSlotCount();
    }

    public void Run()
    {
        var playerEntity = _player.GetEntity();
        var playerTransform = playerEntity.Get<TransformRef>().Transform;

        Vector3 playerPosition = playerTransform.position;

        Vector3 playerForward = playerTransform.forward;
        playerForward.y = 0f;

        float nearestSqrDistance = float.MaxValue;
        int nearestSlotIndex = -1;

        foreach (int i in _slotsFilter)
        {
            ref var slot = ref _slotsFilter.Get1(i);
            if (slot.Actor == null) continue;

            Vector3 toSlot = slot.Actor.transform.position - playerPosition;
            toSlot.y = 0f;

            float sqrDistance = toSlot.sqrMagnitude;
            slot.DistanceToPlayer = Mathf.Sqrt(sqrDistance);

            slot.Position = Vector3.Dot(toSlot, playerForward) >= 0f
                ? SegmentRelativePosition.Ahead
                : SegmentRelativePosition.Behind;

            slot.TimeSinceLastVisit += Time.deltaTime;

            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearestSlotIndex = i;
            }
        }

        if (nearestSlotIndex < 0) return;

        ref var currentSlot = ref _slotsFilter.Get1(nearestSlotIndex);
        currentSlot.Position = SegmentRelativePosition.Current;
        currentSlot.TimeSinceLastVisit = 0f;

        ReportSlotChange(currentSlot.SegmentId);
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportSlotCount()
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[SLOTS] Registered: {_slotsFilter.GetEntitiesCount()}",
            Type = _slotsFilter.GetEntitiesCount() > 0 ? DebugType.Info : DebugType.Error
        };
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportSlotChange(string segmentId)
    {
#if DEV_OVERLAY
        if (_lastReportedSlotId == segmentId) return;
        _lastReportedSlotId = segmentId;

        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[SLOTS] Current: {segmentId}",
            Type = DebugType.Info
        };
#endif
    }
}