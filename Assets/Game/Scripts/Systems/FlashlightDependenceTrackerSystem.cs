using UnityEngine;
using Leopotam.Ecs;
using System.Collections.Generic;

public class FlashlightDependenceTrackerSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private PlayerActor _player;
    private Light _flashlight;

    private float _sampleInterval;
    private float _window;
    private float _minRange;
    private float _maxRange;

    private float _sweepMinAmplitude;
    private float _sweepMaxAmplitude;
    private float _sweepMinSpeed;
    private float _sweepIdleSpeed;
    private float _sweepStrokeBreak;
    private float _sweepRateForMax;

    private float _onWeight;
    private float _sweepWeight;
    private float _focusWeight;

    private float _sampleTimer;

    private bool _wasTracking;
    private float _lastYaw;
    private int _strokeSign;
    private float _strokeAmplitude;
    private float _strokeDuration;
    private float _idleTime;

    private readonly Queue<(float timestamp, bool isOn, float focus)> _usageHistory
        = new Queue<(float timestamp, bool isOn, float focus)>();
    private readonly Queue<float> _sweepEvents = new Queue<float>();

    public void Init()
    {
        _player = SceneData.Player;
        _flashlight = _player.GetEntity().Get<LightRef>().Light;

        var metricsConfig = GameConfig.PlayerMetricsConfig;
        _sampleInterval = metricsConfig.FlashlightSampleInterval;
        _window = metricsConfig.FlashlightDependenceWindow;
        _sweepMinAmplitude = metricsConfig.SweepMinAmplitude;
        _sweepMaxAmplitude = metricsConfig.SweepMaxAmplitude;
        _sweepMinSpeed = metricsConfig.SweepMinSpeed;
        _sweepIdleSpeed = metricsConfig.SweepIdleSpeed;
        _sweepStrokeBreak = metricsConfig.SweepStrokeBreak;
        _sweepRateForMax = metricsConfig.SweepRateForMax;
        _onWeight = metricsConfig.DependenceOnWeight;
        _sweepWeight = metricsConfig.DependenceSweepWeight;
        _focusWeight = metricsConfig.DependenceFocusWeight;

        var playerConfig = GameConfig.PlayerConfig;
        _minRange = playerConfig.FlashlightMinRange;
        _maxRange = playerConfig.FlashlightMaxRange;
    }

    public void Run()
    {
        var playerEntity = _player.GetEntity();

        bool canTrack = _flashlight != null
            && !playerEntity.Has<FreezeFlag>()
            && !playerEntity.Has<DeadFlag>()
            && !playerEntity.Has<FlashlightDischargedFlag>();

        if (!canTrack)
        {
            _wasTracking = false;
            return;
        }

        bool isOn = _flashlight.enabled;
        TrackSweeps(isOn);

        _sampleTimer -= Time.deltaTime;
        if (_sampleTimer > 0f) return;
        _sampleTimer = _sampleInterval;

        ref var metricsComp = ref playerEntity.Get<FlashlightDependenceMetrics>();
        PushUsageSample(isOn);
        RecalculateWindow(ref metricsComp);
    }

    private void TrackSweeps(bool isOn)
    {
        if (!isOn)
        {
            _wasTracking = false;
            return;
        }

        if (!TryGetBeamYaw(out float yaw))
        {
            _wasTracking = false;
            return;
        }

        if (!_wasTracking)
        {
            _wasTracking = true;
            _lastYaw = yaw;
            ResetStroke();
            return;
        }

        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0f) return;

        float delta = Mathf.DeltaAngle(_lastYaw, yaw);
        _lastYaw = yaw;

        float speed = Mathf.Abs(delta) / deltaTime;

        if (speed < _sweepIdleSpeed)
        {
            _idleTime += deltaTime;
            if (_idleTime >= _sweepStrokeBreak) FinishStroke();
            return;
        }

        _idleTime = 0f;
        int sign = delta > 0f ? 1 : -1;

        if (_strokeSign != 0 && sign != _strokeSign) FinishStroke();

        _strokeSign = sign;
        _strokeAmplitude += Mathf.Abs(delta);
        _strokeDuration += deltaTime;
    }

    private void FinishStroke()
    {
        if (_strokeSign != 0 && _strokeDuration > 0f)
        {
            float averageSpeed = _strokeAmplitude / _strokeDuration;
            bool inAmplitudeBand = _strokeAmplitude >= _sweepMinAmplitude && _strokeAmplitude <= _sweepMaxAmplitude;

            if (inAmplitudeBand && averageSpeed >= _sweepMinSpeed) _sweepEvents.Enqueue(Time.time);
        }

        ResetStroke();
    }

    private void ResetStroke()
    {
        _strokeSign = 0;
        _strokeAmplitude = 0f;
        _strokeDuration = 0f;
        _idleTime = 0f;
    }

    private bool TryGetBeamYaw(out float yaw)
    {
        Vector3 forward = _flashlight.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.01f)
        {
            yaw = 0f;
            return false;
        }

        yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        return true;
    }

    private void PushUsageSample(bool isOn)
    {
        float focus = isOn ? Mathf.InverseLerp(_minRange, _maxRange, _flashlight.range) : 0f;
        _usageHistory.Enqueue((Time.time, isOn, focus));
    }

    private void RecalculateWindow(ref FlashlightDependenceMetrics metricsComp)
    {
        while (_usageHistory.Count > 0 && Time.time - _usageHistory.Peek().timestamp > _window)
            _usageHistory.Dequeue();

        while (_sweepEvents.Count > 0 && Time.time - _sweepEvents.Peek() > _window)
            _sweepEvents.Dequeue();

        int total = _usageHistory.Count;
        int onCount = 0;
        float focusSum = 0f;

        foreach (var sample in _usageHistory)
        {
            if (!sample.isOn) continue;
            onCount++;
            focusSum += sample.focus;
        }

        metricsComp.OnRatio = total > 0 ? (float)onCount / total : 0f;

        metricsComp.FocusRatio = total > 0 ? focusSum / total : 0f;

        float sweepsPerMinute = _window > 0f ? _sweepEvents.Count * 60f / _window : 0f;
        metricsComp.SweepsInWindow = _sweepEvents.Count;
        metricsComp.SweepRate = _sweepRateForMax > 0f ? Mathf.Clamp01(sweepsPerMinute / _sweepRateForMax) : 0f;

        metricsComp.Dependence = Mathf.Clamp01(
            metricsComp.OnRatio * _onWeight +
            metricsComp.SweepRate * _sweepWeight +
            metricsComp.FocusRatio * _focusWeight
            );
    }
}