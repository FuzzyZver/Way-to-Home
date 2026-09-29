using UnityEngine;
using Leopotam.Ecs;

public class SegmentFamiliaritySystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<SegmentSlotComponent> _slotsFilter;

    private PlayerActor _player;

    private int _visitsForFull;
    private float _dwellForFull;
    private float _visitWeight;
    private float _dwellWeight;
    private float _enterDebounce;

    private string _confirmedSegmentId;
    private string _candidateSegmentId;
    private float _candidateTime;

    public void Init()
    {
        _player = SceneData.Player;

        var metricsConfig = GameConfig.PlayerMetricsConfig;
        _visitsForFull = metricsConfig.FamiliarityVisitsForFull;
        _dwellForFull = metricsConfig.FamiliarityDwellForFull;
        _visitWeight = metricsConfig.FamiliarityVisitWeight;
        _dwellWeight = metricsConfig.FamiliarityDwellWeight;
        _enterDebounce = metricsConfig.SegmentEnterDebounce;
    }

    public void Run()
    {
        var playerEntity = _player.GetEntity();
        if (playerEntity.Has<DeadFlag>()) return;

        int currentIndex = -1;
        foreach (int i in _slotsFilter)
        {
            ref var slot = ref _slotsFilter.Get1(i);
            if (slot.Position != SegmentRelativePosition.Current) continue;

            currentIndex = i;
            break;
        }

        if (currentIndex < 0) return;

        ref var currentSlot = ref _slotsFilter.Get1(currentIndex);

        currentSlot.DwellTime += Time.deltaTime;
        TryConfirmVisit(ref currentSlot);
        currentSlot.Familiarity = CalculateFamiliarity(in currentSlot);

        // для директора важна фамильярность того места, где игрок стоит сейчас
        ref var metricsComp = ref playerEntity.Get<SegmentFamiliarityMetrics>();
        metricsComp.CurrentFamiliarity = currentSlot.Familiarity;
        metricsComp.CurrentVisitCount = currentSlot.VisitCount;
    }

    private void TryConfirmVisit(ref SegmentSlotComponent currentSlot)
    {
        if (currentSlot.SegmentId == _confirmedSegmentId) return;

        if (currentSlot.SegmentId != _candidateSegmentId)
        {
            _candidateSegmentId = currentSlot.SegmentId;
            _candidateTime = 0f;
        }

        _candidateTime += Time.deltaTime;
        if (_candidateTime < _enterDebounce) return;

        _confirmedSegmentId = currentSlot.SegmentId;
        currentSlot.VisitCount++;
    }

    private float CalculateFamiliarity(in SegmentSlotComponent slot)
    {
        float visitPart = _visitsForFull > 0
            ? Mathf.Clamp01((float)slot.VisitCount / _visitsForFull)
            : 0f;

        float dwellPart = _dwellForFull > 0f
            ? Mathf.Clamp01(slot.DwellTime / _dwellForFull)
            : 0f;

        return Mathf.Clamp01(visitPart * _visitWeight + dwellPart * _dwellWeight);
    }
}