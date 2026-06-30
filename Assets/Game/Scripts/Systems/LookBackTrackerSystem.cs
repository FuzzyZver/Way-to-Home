using UnityEngine;
using Leopotam.Ecs;
using System.Collections.Generic;
using System.Linq;

public class LookBackTrackerSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private PlayerMetricsConfig _playerMetricsConfig;
    private PlayerActor _player;
    private Queue<(float timestamp, float angle)> _angleQueue = new Queue<(float timestamp, float angle)>();
    private List<float> _remainingEvents = new List<float>();

    public void Init()
    {
        _player = SceneData.Player;
        _playerMetricsConfig = GameConfig.PlayerMetricsConfig;
        _player.GetEntity().Get<PlayerLookBackMetrics>().LastLookDirection = _player.GetEntity().Get<CameraTargetRef>().Transform.forward;
    }

    public void Run()
    {
        var playerEntity = _player.GetEntity();
        if (playerEntity.Has<FreezeFlag>()) return;
        if (playerEntity.Has<DeadFlag>()) return;

        LookBackTraching(playerEntity);

        ref var metricsComp = ref playerEntity.Get<PlayerLookBackMetrics>();
        _remainingEvents.RemoveAll(ev => ev >= _playerMetricsConfig.RotationFrequencyWindow);
        float frequency = 0.1f;
        if (_remainingEvents.Count != 0) frequency = Mathf.Clamp01(_remainingEvents.Count / _playerMetricsConfig.RotationFrequencyWindow);
        metricsComp.Frequency = frequency;
        while (_angleQueue.Count > 0 && Time.time - _angleQueue.Peek().timestamp > 1f)
        {
            _angleQueue.Dequeue();
        }
        if(_angleQueue.Count > 0)
        {

            metricsComp.AverageAngleRotation = _angleQueue.Sum(aq => aq.angle) / _angleQueue.Count;
        }
    }

    private void LookBackTraching(EcsEntity playerEntity)
    {
        ref var metricsComp = ref playerEntity.Get<PlayerLookBackMetrics>();

        Transform cameraTransform = playerEntity.Get<CameraTargetRef>().Transform;
        Vector3 lastLookDirection = metricsComp.LastLookDirection;
        float rotationAngle = Vector3.Angle(cameraTransform.forward, lastLookDirection);
        metricsComp.Time += Time.deltaTime;
        _angleQueue.Enqueue((Time.time, rotationAngle));

        if (metricsComp.Time < _playerMetricsConfig.RotationTimeThreshold) return;
        if(rotationAngle >= _playerMetricsConfig.RotationAngleThreshold) _remainingEvents.Add(Time.time);
        metricsComp.Time = 0f;
        metricsComp.LastLookDirection = cameraTransform.forward;
    }
}
