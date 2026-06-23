using UnityEngine;
using Leopotam.Ecs;
using System.Collections.Generic;

public class LightsTrackerSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private PlayerActor _player;

    private float _lightCheckTimer;
    private bool _wasInLightLastFrame;
    private float _startTime;
    private float _raycastCheckInterval = 0.2f;

    private List<float> _levelLightHistory = new List<float>();
    private float _lightThreshold = 0.35f;

    public void Init()
    {
        _startTime = Time.time;
        _wasInLightLastFrame = false;
        _player = SceneData.Player;
    }

    public void Run()
    {
        var playerEntity = _player.GetEntity();
        if (playerEntity.Has<FreezeFlag>()) return;
        if (playerEntity.Has<DeadFlag>()) return;

        _lightCheckTimer -= Time.deltaTime;

        if (_lightCheckTimer <= 0)
        {
            _lightCheckTimer = _raycastCheckInterval;
            UpdateLightsMetrics(playerEntity);
        }
    }

    private void UpdateLightsMetrics(EcsEntity playerEntity)
    {
        ref var lightMetricsComp = ref playerEntity.Get<PlayerLightMetrics>();
        float currentLightLevel = CalculateLightLevel();

        bool isInLight = currentLightLevel >= _lightThreshold;
        lightMetricsComp.IsCurrentlyInLight = isInLight;
        _levelLightHistory.Add(currentLightLevel);

        //Отслеживаю переход от темноты к свету и наоборот
        if (isInLight && !_wasInLightLastFrame)
            lightMetricsComp.DarkToLightTransitions++;
        else if (!isInLight && _wasInLightLastFrame)
            lightMetricsComp.LightToDarkTransitions++;

        //Счет времени в темноте и на свету
        if (isInLight)
            lightMetricsComp.TotalTimeInLight += _raycastCheckInterval;
        else
            lightMetricsComp.TotalTimeInDark += _raycastCheckInterval;

        float sum = 0f;
        foreach(var level in _levelLightHistory)
        {
            sum += level;
        }
        lightMetricsComp.AverageLightLevel = sum / _levelLightHistory.Count > 0 ? sum / _levelLightHistory.Count : 0f;

        float totalTime = lightMetricsComp.TotalTimeInLight + lightMetricsComp.TotalTimeInDark;
        lightMetricsComp.LightPreferencesRatio = totalTime > 0 ? lightMetricsComp.TotalTimeInLight / totalTime : 0.5f;
        _wasInLightLastFrame = isInLight;


    }

    private float CalculateLightLevel()
    {
        Transform playerTransform = _player.GetEntity().Get<TransformRef>().Transform;
        float totalIntensity = 0f;
        int sampleCount = 0;

        Vector3[] samplePoints = new Vector3[]
        {
        playerTransform.position + Vector3.up * 1.7f,
        playerTransform.position,
        playerTransform.position - Vector3.up * 0.5f
        };

        foreach (Vector3 samplePoint in samplePoints)
        {
            foreach (Light light in SceneData.Lights)
            {
                if (light == null || !light.enabled) continue;

                float distance = Vector3.Distance(samplePoint, light.transform.position);
                if (distance > light.range) continue;

                Vector3 dirToLight = (light.transform.position - samplePoint).normalized;
                if (Physics.Raycast(samplePoint, dirToLight, distance))
                    continue;

                float normalizedIntensity = Mathf.Clamp01(light.intensity / 10f);

                float normalizedDistance = distance / light.range;
                float falloff = Mathf.Pow(1f - normalizedDistance, 2f);

                float intensity = normalizedIntensity * falloff;

                if (light.type == LightType.Spot)
                {
                    float angleToLight = Vector3.Angle(light.transform.forward, -dirToLight);
                    if (angleToLight > light.spotAngle * 0.5f) continue;

                    float angleFalloff = 1f - (angleToLight / (light.spotAngle * 0.5f));
                    intensity *= angleFalloff;
                }

                totalIntensity += intensity;
                sampleCount++;
            }
        }

        if (sampleCount > 0)
            totalIntensity /= sampleCount;

        //float ambientLevel = RenderSettings.ambientLight.grayscale;
        //totalIntensity = (totalIntensity * 0.8f) + (ambientLevel * 0.2f);

        return Mathf.Clamp01(totalIntensity);
    }
}
