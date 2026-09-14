using UnityEngine;
using Leopotam.Ecs;

public class RadialScanSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private PlayerActor _player;

    private RadialScanSample[] _samples;
    private int _sampleCount;
    private float _angleStep;

    private float _interval;
    private float _maxDistance;
    private float _eyeHeight;
    private LayerMask _scanMask;

    private float _lastScanTime;

    public void Init()
    {
        _player = SceneData.Player;

        var metricsConfig = GameConfig.PlayerMetricsConfig;
        _interval = metricsConfig.RadialScanInterval;
        _maxDistance = metricsConfig.RadialScanMaxDistance;
        _eyeHeight = metricsConfig.EyeHeight;
        _scanMask = metricsConfig.RadialScanMask;

        _sampleCount = Mathf.Max(4, Mathf.RoundToInt(360f / Mathf.Max(0.5f, metricsConfig.RadialScanAngleStep)));
        _angleStep = 360f / _sampleCount;
        _samples = new RadialScanSample[_sampleCount];
    }

    public void Run()
    {
        if (Time.time - _lastScanTime < _interval) return;
        _lastScanTime = Time.time;

        var playerEntity = _player.GetEntity();
        if (playerEntity.Has<DeadFlag>()) return;

        var playerTransform = playerEntity.Get<TransformRef>().Transform;

        Vector3 origin = playerTransform.position + Vector3.up * _eyeHeight;
        Vector3 forward = playerTransform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) return;
        forward.Normalize();

        for (int i = 0; i < _sampleCount; i++)
        {
            float angle = -180f + i * _angleStep;
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, _maxDistance, _scanMask, QueryTriggerInteraction.Ignore))
            {
                _samples[i] = new RadialScanSample
                {
                    Angle = angle,
                    Distance = hit.distance,
                    Point = hit.point,
                    Collider = hit.collider
                };
            }
            else
            {
                _samples[i] = new RadialScanSample
                {
                    Angle = angle,
                    Distance = _maxDistance,
                    Point = origin + direction * _maxDistance,
                    Collider = null
                };
            }
        }

        EcsWorld.NewEntity().Get<RadialScanEvent>() = new RadialScanEvent
        {
            Samples = _samples,
            Count = _sampleCount,
            AngleStep = _angleStep,
            MaxDistance = _maxDistance,
            Origin = origin,
            Forward = forward
        };
    }
}