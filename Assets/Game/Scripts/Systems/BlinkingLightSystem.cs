using Leopotam.Ecs;
using UnityEngine;

public class BlinkingLightSystem: Injects, IEcsRunSystem
{
    private EcsFilter<BlinkingLightFlag, LightRef> _blinkingLightFlagFilter;

    public void Run()
    {
        foreach(int i in _blinkingLightFlagFilter)
        {
            ref var flagComp = ref _blinkingLightFlagFilter.Get1(i);
            var light = _blinkingLightFlagFilter.Get2(i).Light;

            if(flagComp.Duration > 0)
            {
                flagComp.Duration -= Time.deltaTime;
                if (flagComp.LastBlinkTime <= 0)
                {
                    flagComp.LastBlinkTime = flagComp.Frequency;
                    int probability = Random.Range(0, 100);
                    if (probability >= flagComp.Probability)
                    {
                        light.gameObject.SetActive(false);
                        _blinkingLightFlagFilter.GetEntity(i).Get<ActivatesFlag>() = new ActivatesFlag
                        {
                            Duration = 0.5f,
                            GameObject = light.gameObject
                        };
                    }
                }
                else flagComp.LastBlinkTime -= Time.deltaTime;
            }
            else
            {
                _blinkingLightFlagFilter.GetEntity(i).Del<BlinkingLightFlag>();
            }
        }
    }
}
