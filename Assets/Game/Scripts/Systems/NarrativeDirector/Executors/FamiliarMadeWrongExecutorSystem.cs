using UnityEngine;
using Leopotam.Ecs;
using System.Collections.Generic;
using System.Diagnostics;

public class FamiliarMadeWrongExecutorSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private struct WrongDetailEntry
    {
        public WrongDetailView View;
        public float Remaining;
    }

    private EcsFilter<FamiliarMadeWrongEvent> _wrongEventFilter;
    private EcsFilter<SegmentSlotComponent> _slotsFilter;

    private PlayerActor _player;

    private float _minFamiliarity;
    private int _fallbackDepth;
    private float _revertDelay;
    private float _hiddenCone;
    private float _detailHeight;
    private float _eyeHeight;
    private LayerMask _occlusionMask;

    private readonly List<WrongDetailEntry> _activeDetails = new List<WrongDetailEntry>();
    private readonly List<int> _triedSlots = new List<int>();

    public void Init()
    {
        _player = SceneData.Player;

        var executionConfig = GameConfig.ExecutionConfig;
        _minFamiliarity = executionConfig.WrongMinFamiliarity;
        _fallbackDepth = Mathf.Max(1, executionConfig.WrongFallbackDepth);
        _revertDelay = executionConfig.WrongRevertDelay;
        _hiddenCone = executionConfig.WrongHiddenCone;
        _detailHeight = executionConfig.WrongDetailHeight;

        var metricsConfig = GameConfig.PlayerMetricsConfig;
        _eyeHeight = metricsConfig.EyeHeight;
        _occlusionMask = metricsConfig.RadialScanMask;
    }

    public void Run()
    {
        var playerEntity = _player.GetEntity();
        var playerTransform = playerEntity.Get<TransformRef>().Transform;

        Vector3 eye = playerTransform.position + Vector3.up * _eyeHeight;
        Vector3 forward = playerTransform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude > 0.001f) forward.Normalize();

        TickRevert(eye, forward);

        foreach (int i in _wrongEventFilter)
        {
            ref var wrongEvent = ref _wrongEventFilter.Get1(i);
            MakeWrong(in wrongEvent, eye, forward);
        }
    }

    private void MakeWrong(in FamiliarMadeWrongEvent wrongEvent, Vector3 eye, Vector3 forward)
    {
        float minFamiliarity = wrongEvent.MinFamiliarity > 0f ? wrongEvent.MinFamiliarity : _minFamiliarity;

        if (!TrySelectDetail(minFamiliarity, eye, forward, out WrongDetailView view, out string segmentId, out int rank))
        {
            ReportSkip($"No hidden detail in the {_fallbackDepth} most familiar segments");
            return;
        }

        view.NormalState.SetActive(false);
        view.WrongState.SetActive(true);

        if (_revertDelay > 0f)
        {
            _activeDetails.Add(new WrongDetailEntry
            {
                View = view,
                Remaining = _revertDelay
            });
        }

        ReportChanged(segmentId, view.gameObject.name, rank);
    }

    private bool TrySelectDetail(float minFamiliarity, Vector3 eye, Vector3 forward,
        out WrongDetailView result, out string segmentId, out int rank)
    {
        result = null;
        segmentId = null;
        rank = 0;

        _triedSlots.Clear();

        for (int attempt = 0; attempt < _fallbackDepth; attempt++)
        {
            int bestIndex = FindNextFamiliarSlot(minFamiliarity);
            if (bestIndex < 0) break;

            _triedSlots.Add(bestIndex);

            ref var slot = ref _slotsFilter.Get1(bestIndex);
            var candidate = PickFromSlot(in slot, eye, forward);
            if (candidate == null) continue;

            result = candidate;
            segmentId = slot.SegmentId;
            rank = attempt + 1;
            return true;
        }

        return false;
    }

    /// <summary>Самый знакомый из ещё не проверенных сегментов выше порога.</summary>
    private int FindNextFamiliarSlot(float minFamiliarity)
    {
        int bestIndex = -1;
        float bestFamiliarity = minFamiliarity;

        foreach (int i in _slotsFilter)
        {
            if (_triedSlots.Contains(i)) continue;

            ref var slot = ref _slotsFilter.Get1(i);
            if (slot.Objects == null) continue;
            if (slot.Familiarity < bestFamiliarity) continue;

            bestFamiliarity = slot.Familiarity;
            bestIndex = i;
        }

        return bestIndex;
    }

    private WrongDetailView PickFromSlot(in SegmentSlotComponent slot, Vector3 eye, Vector3 forward)
    {
        WrongDetailView picked = null;
        int candidateCount = 0;

        for (int i = 0; i < slot.Objects.Count; i++)
        {
            var slotObject = slot.Objects[i];

            if (slotObject.Type != SegmentObjectsType.WrongDetail) continue;
            if (slotObject.Object == null) continue;
            if (!slotObject.Object.activeInHierarchy) continue;
            if (!slotObject.Object.TryGetComponent(out WrongDetailView view)) continue;
            if (!view.IsValid) continue;
            if (view.WrongState.activeSelf) continue;
            if (IsInSight(eye, forward, view.transform.position)) continue;

            candidateCount++;
            if (Random.Range(0, candidateCount) == 0) picked = view;
        }

        return picked;
    }

    private void TickRevert(Vector3 eye, Vector3 forward)
    {
        for (int i = _activeDetails.Count - 1; i >= 0; i--)
        {
            var entry = _activeDetails[i];

            if (entry.View == null)
            {
                _activeDetails.RemoveAt(i);
                continue;
            }

            entry.Remaining -= Time.deltaTime;
            _activeDetails[i] = entry;

            if (entry.Remaining > 0f) continue;

            // возвращать на глазах нельзя — это читается как баг, а не как наваждение
            if (IsInSight(eye, forward, entry.View.transform.position)) continue;

            entry.View.WrongState.SetActive(false);
            entry.View.NormalState.SetActive(true);
            _activeDetails.RemoveAt(i);

            ReportReverted(entry.View.gameObject.name);
        }
    }

    private bool IsInSight(Vector3 eye, Vector3 forward, Vector3 detailPosition)
    {
        Vector3 target = detailPosition + Vector3.up * _detailHeight;
        Vector3 toTarget = target - eye;

        Vector3 flat = toTarget;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.001f) return true;
        if (Vector3.Angle(forward, flat) > _hiddenCone * 0.5f) return false;

        float distance = toTarget.magnitude;
        return !Physics.Raycast(eye, toTarget / distance, distance - 0.2f, _occlusionMask, QueryTriggerInteraction.Ignore);
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportChanged(string segmentId, string detailName, int rank)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[FAMILIAR MADE WRONG] {detailName} in {segmentId} (rank {rank})",
            Type = DebugType.Info
        };
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportReverted(string detailName)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[FAMILIAR MADE WRONG] Reverted {detailName}",
            Type = DebugType.Info
        };
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportSkip(string reason)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[FAMILIAR MADE WRONG] Skipped: {reason}",
            Type = DebugType.Warning
        };
    }
}