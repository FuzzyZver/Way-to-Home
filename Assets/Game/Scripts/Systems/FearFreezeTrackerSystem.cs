using UnityEngine;
using Leopotam.Ecs;
using System.Collections.Generic;
using System.Linq;

public class FearFreezeTrackerSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private PlayerMetricsConfig _playerMetricsConfig;
    private PlayerActor _player;
    private Queue<(float timestamp, float angle, float distance)> _activityQueue = new Queue<(float timestamp, float angle, float distance)>();
        
    public void Init()
    {
        _player = SceneData.Player;
        _player.GetEntity().Get<FearFreezeMetrics>().LastLookDirection = _player.GetEntity().Get<CameraTargetRef>().Transform.forward;
        _playerMetricsConfig = GameConfig.PlayerMetricsConfig;
    }

    public void Run()
    {
        var playerEntity = _player.GetEntity();
        if (playerEntity.Has<FreezeFlag>()) return;
        if (playerEntity.Has<DeadFlag>()) return;
        FreezeTraching(playerEntity);

        while (_activityQueue.Count > 0 && Time.time - _activityQueue.Peek().timestamp > _playerMetricsConfig.FreezeFrequencyWindow)
        {
            _activityQueue.Dequeue();
        }
        if (_activityQueue.Count > 0)
        {
            ref var metricsComp = ref playerEntity.Get<FearFreezeMetrics>();
            float averageAngle  = _activityQueue.Sum(aq => aq.angle) / _activityQueue.Count;
            float averageDistance = _activityQueue.Sum(aq => aq.distance) / _activityQueue.Count;

            float normalizedAngle = Mathf.Clamp01(averageAngle / _playerMetricsConfig.FreezeAngleThreshold);
            float normalizedDistance = Mathf.Clamp01(averageDistance / _playerMetricsConfig.FreezeDistanceThreshold);
            float fearFreeze = Mathf.Clamp01(normalizedAngle * normalizedDistance);
            if(fearFreeze > _playerMetricsConfig.FearFreezeThreshold)
            {
                metricsComp.FearFreeze = fearFreeze;
            }
            else
            {
                metricsComp.FearFreeze = 1f;
            }
        }
    }

    private void FreezeTraching(EcsEntity playerEntity)
    {
        ref var metricsComp = ref playerEntity.Get<FearFreezeMetrics>();
        var playerTransform = playerEntity.Get<TransformRef>().Transform;
        Transform cameraTransform = playerEntity.Get<CameraTargetRef>().Transform;
        Vector3 lastLookDirection = metricsComp.LastLookDirection;

        float rotationAngle = Vector3.Angle(cameraTransform.forward, lastLookDirection);
        float distance = Vector3.Distance(playerTransform.position, metricsComp.LastPosition);        
        metricsComp.Time += Time.deltaTime;
        
        if (metricsComp.Time < _playerMetricsConfig.FreezeTimeTrheshold) return;
        if (distance < _playerMetricsConfig.FreezeDistanceThreshold &&
            rotationAngle < _playerMetricsConfig.FreezeAngleThreshold)
        {
            _activityQueue.Enqueue((Time.time, rotationAngle, distance));
        }
        metricsComp.LastLookDirection = cameraTransform.forward;
        metricsComp.LastPosition = playerTransform.position;
        metricsComp.Time = 0;
    }
}