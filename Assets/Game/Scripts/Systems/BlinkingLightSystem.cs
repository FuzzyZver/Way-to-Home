using Leopotam.Ecs;
using UnityEngine;

public class BlinkingLightSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<BlinkingLightFlag, LightRef> _blinkingLightFlagFilter;

    private float _calmMin;
    private float _calmMax;
    private float _burstMin;
    private float _burstMax;
    private float _toggleMin;
    private float _toggleMax;
    private float _calmJitter;
    private float _calmNoiseSpeed;
    private float _burstPeak;
    private float _burstFloor;

    public void Init()
    {
        var executionConfig = GameConfig.ExecutionConfig;
        _calmMin = executionConfig.BlinkCalmMin;
        _calmMax = executionConfig.BlinkCalmMax;
        _burstMin = executionConfig.BlinkBurstMin;
        _burstMax = executionConfig.BlinkBurstMax;
        _toggleMin = executionConfig.BlinkToggleMin;
        _toggleMax = executionConfig.BlinkToggleMax;
        _calmJitter = executionConfig.BlinkCalmJitter;
        _calmNoiseSpeed = executionConfig.BlinkCalmNoiseSpeed;
        _burstPeak = executionConfig.BlinkBurstPeak;
        _burstFloor = executionConfig.BlinkBurstFloor;
    }

    public void Run()
    {
        foreach (int i in _blinkingLightFlagFilter)
        {
            ref var flagComp = ref _blinkingLightFlagFilter.Get1(i);
            var light = _blinkingLightFlagFilter.Get2(i).Light;
            var entity = _blinkingLightFlagFilter.GetEntity(i);

            if (light == null)
            {
                entity.Del<BlinkingLightFlag>();
                continue;
            }

            if (!flagComp.Initialized)
            {
                flagComp.Initialized = true;
                flagComp.BaseIntensity = light.intensity;
                flagComp.NoiseSeed = Random.Range(0f, 100f);
                EnterCalm(ref flagComp);
            }

            flagComp.Duration -= Time.deltaTime;
            if (flagComp.Duration <= 0f)
            {
                light.enabled = true;
                light.intensity = flagComp.BaseIntensity;
                entity.Del<BlinkingLightFlag>();
                continue;
            }

            flagComp.StateRemaining -= Time.deltaTime;

            if (flagComp.InBurst) TickBurst(ref flagComp, light);
            else TickCalm(ref flagComp, light);
        }
    }

    private void TickCalm(ref BlinkingLightFlag flagComp, Light light)
    {
        float noise = Mathf.PerlinNoise(Time.time * _calmNoiseSpeed, flagComp.NoiseSeed);

        light.enabled = true;
        light.intensity = flagComp.BaseIntensity * Mathf.Lerp(1f - _calmJitter, 1f, noise);

        if (flagComp.StateRemaining > 0f) return;
        EnterBurst(ref flagComp);
    }

    private void TickBurst(ref BlinkingLightFlag flagComp, Light light)
    {
        flagComp.ToggleRemaining -= Time.deltaTime;
        if (flagComp.ToggleRemaining <= 0f)
        {
            flagComp.BurstOn = !flagComp.BurstOn;
            flagComp.ToggleRemaining = Random.Range(_toggleMin, _toggleMax);
        }

        light.enabled = true;
        light.intensity = flagComp.BaseIntensity * (flagComp.BurstOn ? _burstPeak : _burstFloor);

        if (flagComp.StateRemaining > 0f) return;
        EnterCalm(ref flagComp);
    }

    private void EnterCalm(ref BlinkingLightFlag flagComp)
    {
        flagComp.InBurst = false;
        flagComp.StateRemaining = Random.Range(_calmMin, _calmMax);
    }

    private void EnterBurst(ref BlinkingLightFlag flagComp)
    {
        flagComp.InBurst = true;
        flagComp.StateRemaining = Random.Range(_burstMin, _burstMax);
        flagComp.ToggleRemaining = 0f;
        flagComp.BurstOn = false;
    }
}