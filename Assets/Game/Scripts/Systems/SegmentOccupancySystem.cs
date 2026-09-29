using UnityEngine;
using Leopotam.Ecs;

/// <summary>
/// Переводит сырые события триггер-объёмов в состояние занятости сегмента.
/// Единственный владелец полей OverlapCount и LastEnterTime.
/// Регистрировать перед DistanceToPlayerSystem — она по ним выбирает текущий слот.
/// </summary>
public class SegmentOccupancySystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<SegmentTriggerEvent> _triggerEventFilter;

    private Rigidbody _playerRigidbody;

    public void Init()
    {
        _playerRigidbody = SceneData.Player.GetEntity().Get<RigidbodyRef>().Rigidbody;
    }

    public void Run()
    {
        foreach (int i in _triggerEventFilter)
        {
            ref var triggerEvent = ref _triggerEventFilter.Get1(i);

            if (!IsPlayer(triggerEvent.Other)) continue;
            if (!triggerEvent.Segment.IsAlive()) continue;

            ref var slot = ref triggerEvent.Segment.Get<SegmentSlotComponent>();

            if (triggerEvent.Entered)
            {
                slot.OverlapCount++;
                slot.LastEnterTime = Time.time;
                continue;
            }

            slot.OverlapCount = Mathf.Max(0, slot.OverlapCount - 1);
        }
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null) return false;

        return other.attachedRigidbody == _playerRigidbody;
    }
}