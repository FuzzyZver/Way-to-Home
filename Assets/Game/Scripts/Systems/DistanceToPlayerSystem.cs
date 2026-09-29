using UnityEngine;
using Leopotam.Ecs;
using System.Diagnostics;

public class DistanceToPlayerSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<SegmentSlotComponent> _slotsFilter;

    private PlayerActor _player;
    private EcsEntity _currentSlot;

#if DEV_OVERLAY
    private string _lastReportedSlotId;
    private bool _wasOutside;
#endif

    public void Init()
    {
        _player = SceneData.Player;
        _currentSlot = EcsEntity.Null;

        CalculateBounds();
        ReportSlotCount();
    }
    private void CalculateBounds()
    {
        foreach (int i in _slotsFilter)
        {
            ref var slot = ref _slotsFilter.Get1(i);
            if (slot.Actor == null) continue;

            var volumes = slot.Actor.GetComponentsInChildren<SegmentVolume>(true);

            bool found = false;
            Bounds bounds = new Bounds(slot.Actor.transform.position, Vector3.one);

            for (int v = 0; v < volumes.Length; v++)
            {
                if (!volumes[v].isActiveAndEnabled) continue;
                if (!volumes[v].TryGetComponent(out Collider volumeCollider)) continue;

                if (!found)
                {
                    bounds = volumeCollider.bounds;
                    found = true;
                    continue;
                }

                bounds.Encapsulate(volumeCollider.bounds);
            }

            slot.Bounds = bounds;
            ReportMissingVolumes(slot.SegmentId, found);
        }
    }

    public void Run()
    {
        var playerEntity = _player.GetEntity();
        var playerTransform = playerEntity.Get<TransformRef>().Transform;

        Vector3 playerPosition = playerTransform.position;
        Vector3 playerForward = playerTransform.forward;
        playerForward.y = 0f;

        var occupiedSlot = FindOccupiedSlot();

        if (occupiedSlot != EcsEntity.Null) _currentSlot = occupiedSlot;
        ReportOutside(occupiedSlot == EcsEntity.Null);

        foreach (int i in _slotsFilter)
        {
            ref var slot = ref _slotsFilter.Get1(i);

            slot.DistanceToPlayer = Mathf.Sqrt(slot.Bounds.SqrDistance(playerPosition));

            Vector3 toSlot = slot.Bounds.center - playerPosition;
            toSlot.y = 0f;

            slot.Position = Vector3.Dot(toSlot, playerForward) >= 0f
                ? SegmentRelativePosition.Ahead
                : SegmentRelativePosition.Behind;

            slot.TimeSinceLastVisit += Time.deltaTime;
        }

        if (_currentSlot == EcsEntity.Null) return;

        ref var currentSlot = ref _currentSlot.Get<SegmentSlotComponent>();
        currentSlot.Position = SegmentRelativePosition.Current;
        currentSlot.TimeSinceLastVisit = 0f;

        ReportSlotChange(currentSlot.SegmentId);
    }

    private EcsEntity FindOccupiedSlot()
    {
        var bestSlot = EcsEntity.Null;
        float lastEnterTime = float.MinValue;

        foreach (int i in _slotsFilter)
        {
            ref var slot = ref _slotsFilter.Get1(i);
            if (slot.OverlapCount <= 0) continue;
            if (slot.LastEnterTime < lastEnterTime) continue;

            lastEnterTime = slot.LastEnterTime;
            bestSlot = _slotsFilter.GetEntity(i);
        }

        return bestSlot;
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
    private void ReportMissingVolumes(string segmentId, bool found)
    {
        if (found) return;

        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[SLOTS] {segmentId} has no volumes, falling back to its pivot",
            Type = DebugType.Error
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

    [Conditional("DEV_OVERLAY")]
    private void ReportOutside(bool isOutside)
    {
#if DEV_OVERLAY
        if (isOutside == _wasOutside) return;
        _wasOutside = isOutside;

        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = isOutside
                ? "[SLOTS] Player is outside every segment volume, holding last known"
                : "[SLOTS] Player is back inside a segment volume",
            Type = isOutside ? DebugType.Warning : DebugType.Info
        };
#endif
    }
}