using UnityEngine;
using UnityEngine.Pool;
using Leopotam.Ecs;
using System.Collections.Generic;
using System.Diagnostics;

public class StalkerGlimpseExecutorSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<StalkerGlimpseEvent> _glimpseEventFilter;
    private EcsFilter<RadialScanEvent> _radialScanFilter;
    private EcsFilter<SegmentSlotComponent> _slotsFilter;
    private EcsFilter<StalkerShadowComponent, TransformRef> _activeShadowsFilter;
    private EcsFilter<DespawnFlag, StalkerShadowRef> _shadowDespawnFilter;

    private PlayerActor _player;
    private ObjectPool<StalkerShadowActor> _shadowPool;
    private StalkerShadowActor _shadowPrefab;

    private float _defaultDuration;
    private int _defaultSlotCount;
    private float _minSpawnDistance;
    private float _maxSpawnDistance;
    private float _spawnHiddenCone;
    private float _gazeCone;
    private float _gazeDwellTime;
    private float _vanishSpeedOnGaze;
    private float _vanishSpeedOnTimeout;
    private float _chestHeight;
    private float _eyeHeight;
    private LayerMask _occlusionMask;

    private RadialScanSample[] _scanSamples;
    private int _scanCount;
    private float _scanAngleStep;
    private Vector3 _scanForward;

    private readonly List<int> _targetSlots = new List<int>();

    public void Init()
    {
        _player = SceneData.Player;

        var executionConfig = GameConfig.ExecutionConfig;
        _shadowPrefab = executionConfig.ShadowActor;
        _defaultDuration = executionConfig.StalkerGlimpseDuration;
        _defaultSlotCount = executionConfig.StalkerGlimpseSlots;
        _minSpawnDistance = executionConfig.ShadowMinSpawnDistance;
        _maxSpawnDistance = executionConfig.ShadowMaxSpawnDistance;
        _spawnHiddenCone = executionConfig.ShadowSpawnHiddenCone;
        _gazeCone = executionConfig.ShadowGazeCone;
        _gazeDwellTime = executionConfig.ShadowGazeDwellTime;
        _vanishSpeedOnGaze = executionConfig.ShadowVanishSpeedOnGaze;
        _vanishSpeedOnTimeout = executionConfig.ShadowVanishSpeedOnTimeout;
        _chestHeight = executionConfig.ShadowChestHeight;

        var metricsConfig = GameConfig.PlayerMetricsConfig;
        _eyeHeight = metricsConfig.EyeHeight;
        _occlusionMask = metricsConfig.RadialScanMask;

        _shadowPool = new ObjectPool<StalkerShadowActor>(
            CreateShadow,
            OnShadowTaken,
            OnShadowReleased,
            OnShadowDestroyed,
            true, 
            4,
            16
            );
    }

    public void Run()
    {
        CacheRadialScan();

        var playerEntity = _player.GetEntity();
        var playerTransform = playerEntity.Get<TransformRef>().Transform;

        Vector3 eye = playerTransform.position + Vector3.up * _eyeHeight;
        Vector3 forward = playerTransform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude > 0.001f) forward.Normalize();

        foreach (int i in _glimpseEventFilter)
        {
            ref var glimpseEvent = ref _glimpseEventFilter.Get1(i);
            SpawnGlimpse(in glimpseEvent, eye, forward);
        }

        UpdateActiveShadows(eye, forward);
        ReleaseDespawned();
    }

    private void CacheRadialScan()
    {
        foreach (int i in _radialScanFilter)
        {
            ref var scan = ref _radialScanFilter.Get1(i);
            if (scan.Samples == null || scan.Count <= 0) continue;

            if (_scanSamples == null || _scanSamples.Length < scan.Count)
                _scanSamples = new RadialScanSample[scan.Count];

            System.Array.Copy(scan.Samples, _scanSamples, scan.Count);
            _scanCount = scan.Count;
            _scanAngleStep = scan.AngleStep;
            _scanForward = scan.Forward;
        }
    }

    private void SpawnGlimpse(in StalkerGlimpseEvent glimpseEvent, Vector3 eye, Vector3 forward)
    {
        float duration = glimpseEvent.Duration > 0f ? glimpseEvent.Duration : _defaultDuration;
        int slotLimit = glimpseEvent.SlotCount > 0 ? glimpseEvent.SlotCount : _defaultSlotCount;

        CollectTargetSlots(slotLimit);

        int spawned = 0;
        for (int i = 0; i < _targetSlots.Count; i++)
        {
            if (!TrySelectSpawnPoint(_targetSlots[i], eye, forward, out Vector3 point)) continue;

            SpawnShadow(point, eye, duration);
            spawned++;
        }

        ReportSpawn(spawned, _targetSlots.Count);
    }

    private void CollectTargetSlots(int limit)
    {
        _targetSlots.Clear();

        foreach (int i in _slotsFilter)
        {
            ref var slot = ref _slotsFilter.Get1(i);
            if (slot.Position != SegmentRelativePosition.Current) continue;

            _targetSlots.Add(i);
            break;
        }

        while (_targetSlots.Count < limit)
        {
            int bestIndex = -1;
            float bestDistance = float.MaxValue;

            foreach (int i in _slotsFilter)
            {
                ref var slot = ref _slotsFilter.Get1(i);
                if (slot.Position != SegmentRelativePosition.Ahead) continue;
                if (slot.DistanceToPlayer >= bestDistance) continue;
                if (_targetSlots.Contains(i)) continue;

                bestDistance = slot.DistanceToPlayer;
                bestIndex = i;
            }

            if (bestIndex < 0) break;
            _targetSlots.Add(bestIndex);
        }
    }

    private bool TrySelectSpawnPoint(int slotIndex, Vector3 eye, Vector3 forward, out Vector3 result)
    {
        result = Vector3.zero;

        ref var slot = ref _slotsFilter.Get1(slotIndex);
        if (slot.Objects == null) return false;

        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < slot.Objects.Count; i++)
        {
            var slotObject = slot.Objects[i];

            if (slotObject.Type != SegmentObjectsType.ShadowSpawnPoint) continue;
            if (slotObject.Object == null) continue;
            if (!slotObject.Object.activeInHierarchy) continue;

            Vector3 point = slotObject.Object.transform.position;
            float distance = Vector3.Distance(eye, point);

            if (distance < _minSpawnDistance || distance > _maxSpawnDistance) continue;

            if (IsInSight(eye, forward, point, _spawnHiddenCone)) continue;

            float score = EvaluateOpenness(eye, point, distance);
            if (score <= bestScore) continue;

            bestScore = score;
            result = point;
        }

        return bestScore > float.NegativeInfinity;
    }

    private float EvaluateOpenness(Vector3 eye, Vector3 point, float distance)
    {
        if (_scanCount == 0 || _scanAngleStep <= 0f) return distance;

        Vector3 flat = point - eye;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.001f) return distance;

        float angle = Vector3.SignedAngle(_scanForward, flat, Vector3.up);
        int index = Mathf.Clamp(Mathf.RoundToInt((angle + 180f) / _scanAngleStep), 0, _scanCount - 1);

        return _scanSamples[index].Distance;
    }

    private void SpawnShadow(Vector3 point, Vector3 eye, float duration)
    {
        var shadow = _shadowPool.Get();
        shadow.transform.position = point;

        Vector3 toPlayer = eye - point;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.001f) shadow.transform.rotation = Quaternion.LookRotation(toPlayer);

        var entity = shadow.GetEntity();
        if (!entity.IsAlive())
        {
            shadow.Init(EcsWorld);
            entity = shadow.GetEntity();
        }

        entity.Get<StalkerShadowRef>().Actor = shadow;

        if (entity.Has<FadeComponent>()) entity.Del<FadeComponent>();
        if (entity.Has<DespawnFlag>()) entity.Del<DespawnFlag>();

        entity.Get<StalkerShadowComponent>() = new StalkerShadowComponent
        {
            Remaining = duration,
            GazeTime = 0f
        };
    }

    private void UpdateActiveShadows(Vector3 eye, Vector3 forward)
    {
        foreach (int i in _activeShadowsFilter)
        {
            ref var shadowComp = ref _activeShadowsFilter.Get1(i);
            var shadowTransform = _activeShadowsFilter.Get2(i).Transform;
            var entity = _activeShadowsFilter.GetEntity(i);

            shadowComp.Remaining -= Time.deltaTime;
            bool expired = shadowComp.Remaining <= 0f;
            bool caught = false;

            if (!expired)
            {
                if (IsInSight(eye, forward, shadowTransform.position, _gazeCone))
                {
                    shadowComp.GazeTime += Time.deltaTime;
                    caught = shadowComp.GazeTime >= _gazeDwellTime;
                }
                else
                {
                    shadowComp.GazeTime = 0f;
                }
            }

            if (!expired && !caught) continue;

            entity.Del<StalkerShadowComponent>();
            entity.Get<FadeComponent>() = new FadeComponent
            {
                Current = 1f,
                Target = 0f,
                Speed = caught ? _vanishSpeedOnGaze : _vanishSpeedOnTimeout
            };

            ReportVanish(caught);
        }
    }

    private bool IsInSight(Vector3 eye, Vector3 forward, Vector3 groundPoint, float coneAngle)
    {
        Vector3 target = groundPoint + Vector3.up * _chestHeight;
        Vector3 toTarget = target - eye;

        Vector3 flat = toTarget;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.001f) return true;
        if (Vector3.Angle(forward, flat) > coneAngle * 0.5f) return false;

        float distance = toTarget.magnitude;
        return !Physics.Raycast(eye, toTarget / distance, distance - 0.2f, _occlusionMask, QueryTriggerInteraction.Ignore);
    }

    private void ReleaseDespawned()
    {
        foreach (int i in _shadowDespawnFilter)
        {
            var entity = _shadowDespawnFilter.GetEntity(i);
            var shadow = _shadowDespawnFilter.Get2(i).Actor;

            entity.Del<DespawnFlag>();
            if (shadow != null) _shadowPool.Release(shadow);
        }
    }

    private StalkerShadowActor CreateShadow()
    {
        var shadow = GameObject.Instantiate(_shadowPrefab);
        shadow.Init(EcsWorld);
        return shadow;
    }

    private void OnShadowTaken(StalkerShadowActor shadow)
    {
        shadow.gameObject.SetActive(true);
        shadow.ResetDissolve();
    }

    private void OnShadowReleased(StalkerShadowActor shadow)
    {
        shadow.gameObject.SetActive(false);
    }

    private void OnShadowDestroyed(StalkerShadowActor shadow)
    {
        var entity = shadow.GetEntity();
        if (entity.IsAlive()) entity.Destroy();
        GameObject.Destroy(shadow.gameObject);
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportSpawn(int spawned, int slots)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[STALKER GLIMPSE] Spawned {spawned}/{slots} shadow(s)",
            Type = spawned > 0 ? DebugType.Info : DebugType.Warning
        };
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportVanish(bool caught)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = caught ? "[STALKER GLIMPSE] Shadow caught by gaze" : "[STALKER GLIMPSE] Shadow timed out",
            Type = DebugType.Info
        };
    }
}