using UnityEngine;
using Leopotam.Ecs;
using System.Collections.Generic;

public class LightsTrackerSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<SegmentSlotComponent> _slotsFilter;

    private PlayerActor _player;

    private float _lightCheckTimer;
    private bool _wasInLightLastCheck;

    private float _raycastCheckInterval;
    private float _lightThreshold;
    private float _spotAngleMultiplier;
    private float _lightIntensityNormality;
    private float _lightPreferenceWindow;
    private LayerMask _occlusionMask;

    private readonly List<Light> _slotLights = new List<Light>();
    private readonly Queue<(float timestamp, float level, bool inLight)> _lightHistory
        = new Queue<(float timestamp, float level, bool inLight)>();
    private readonly Vector3[] _samplePoints = new Vector3[2];

    public void Init()
    {
        _player = SceneData.Player;
        _wasInLightLastCheck = false;

        var metricsConfig = GameConfig.PlayerMetricsConfig;
        _raycastCheckInterval = metricsConfig.RaycastCheckInterval;
        _lightThreshold = metricsConfig.LightThreshold;
        _spotAngleMultiplier = metricsConfig.SpotAngleMultiplier;
        _lightIntensityNormality = metricsConfig.LightIntensityNormality;
        _lightPreferenceWindow = metricsConfig.LightPreferenceWindow;
        _occlusionMask = metricsConfig.LightOcclusionMask;

        _player.GetEntity().Get<PlayerLightMetrics>().LightPreferencesRatio = 0.5f;
    }

    public void Run()
    {
        var playerEntity = _player.GetEntity();
        if (playerEntity.Has<FreezeFlag>()) return;
        if (playerEntity.Has<DeadFlag>()) return;

        _lightCheckTimer -= Time.deltaTime;
        if (_lightCheckTimer > 0f) return;

        _lightCheckTimer = _raycastCheckInterval;
        UpdateLightsMetrics(playerEntity);
    }

    private void UpdateLightsMetrics(EcsEntity playerEntity)
    {
        ref var lightMetricsComp = ref playerEntity.Get<PlayerLightMetrics>();
        var playerTransform = playerEntity.Get<TransformRef>().Transform;

        CollectNearbyLights();

        float currentLightLevel = CalculateLightLevel(playerTransform);
        bool isInLight = currentLightLevel >= _lightThreshold;

        lightMetricsComp.IsCurrentlyInLight = isInLight;

        if (isInLight && !_wasInLightLastCheck)
            lightMetricsComp.DarkToLightTransitions++;
        else if (!isInLight && _wasInLightLastCheck)
            lightMetricsComp.LightToDarkTransitions++;

        _wasInLightLastCheck = isInLight;

        if (isInLight)
            lightMetricsComp.TotalTimeInLight += _raycastCheckInterval;
        else
            lightMetricsComp.TotalTimeInDark += _raycastCheckInterval;

        PushSample(currentLightLevel, isInLight);
        RecalculateWindow(ref lightMetricsComp);
    }

    private void PushSample(float level, bool isInLight)
    {
        _lightHistory.Enqueue((Time.time, level, isInLight));

        while (_lightHistory.Count > 0 &&
               Time.time - _lightHistory.Peek().timestamp > _lightPreferenceWindow)
        {
            _lightHistory.Dequeue();
        }
    }

    private void RecalculateWindow(ref PlayerLightMetrics lightMetricsComp)
    {
        if (_lightHistory.Count == 0)
        {
            lightMetricsComp.AverageLightLevel = 0f;
            lightMetricsComp.LightPreferencesRatio = 0.5f;
            return;
        }

        float levelSum = 0f;
        int inLightCount = 0;

        foreach (var sample in _lightHistory)
        {
            levelSum += sample.level;
            if (sample.inLight) inLightCount++;
        }

        int count = _lightHistory.Count;
        lightMetricsComp.AverageLightLevel = levelSum / count;
        lightMetricsComp.LightPreferencesRatio = (float)inLightCount / count;
    }

    private void CollectNearbyLights()
    {
        _slotLights.Clear();

        foreach (int i in _slotsFilter)
        {
            ref var slot = ref _slotsFilter.Get1(i);
            if (slot.Position == SegmentRelativePosition.Behind) continue;
            if (slot.Objects == null) continue;

            for (int j = 0; j < slot.Objects.Count; j++)
            {
                var slotObject = slot.Objects[j];

                if (slotObject.Type != SegmentObjectsType.Light) continue;
                if (slotObject.Object == null) continue;
                if (!slotObject.Object.activeInHierarchy) continue;
                if (!slotObject.Object.TryGetComponent(out Light light)) continue;
                if (!light.enabled) continue;
                if (light.type != LightType.Point && light.type != LightType.Spot) continue;

                _slotLights.Add(light);
            }
        }
    }

    private float CalculateLightLevel(Transform playerTransform)
    {
        if (_slotLights.Count == 0) return 0f;

        _samplePoints[0] = playerTransform.position + Vector3.up * 1.7f;
        _samplePoints[1] = playerTransform.position + Vector3.up * 0.9f;

        float totalIntensity = 0f;

        for (int s = 0; s < _samplePoints.Length; s++)
        {
            Vector3 samplePoint = _samplePoints[s];

            for (int l = 0; l < _slotLights.Count; l++)
            {
                Light light = _slotLights[l];

                Vector3 toLight = light.transform.position - samplePoint;
                float distance = toLight.magnitude;

                if (distance > light.range) continue;
                if (distance < 0.001f) continue;

                Vector3 dirToLight = toLight / distance;

                float normalizedDistance = distance / light.range;
                float intensity = Mathf.Clamp01(light.intensity / _lightIntensityNormality)
                                * Mathf.Exp(-normalizedDistance * normalizedDistance);

                if (light.type == LightType.Spot)
                {
                    float coneLimit = light.spotAngle * _spotAngleMultiplier;
                    if (coneLimit <= 0f) continue;

                    float angleToLight = Vector3.Angle(light.transform.forward, -dirToLight);
                    if (angleToLight > coneLimit) continue;

                    intensity *= 1f - (angleToLight / coneLimit);
                }

                if (intensity <= 0.001f) continue;
                
                if (Physics.Raycast(samplePoint, dirToLight, distance, _occlusionMask, QueryTriggerInteraction.Ignore))
                    continue;

                totalIntensity += intensity;
            }
        }
        
        return Mathf.Clamp01(totalIntensity / _samplePoints.Length);
    }
}