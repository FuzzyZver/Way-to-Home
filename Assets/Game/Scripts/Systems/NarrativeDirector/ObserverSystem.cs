using UnityEngine;
using Leopotam.Ecs;

public class ObserverSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private PlayerActor _player;
    private EcsEntity _observer;
    
    public void Init()
    {
        _player = SceneData.Player;
        _observer = EcsWorld.NewEntity();
        _observer.Get<PlayerModel>();
    }

    public void Run()
    {
        var playerEntity = _player.GetEntity();
        ref var lightMetricComp = ref playerEntity.Get<PlayerLightMetrics>();
        ref var lookBackMetricsComp = ref playerEntity.Get<PlayerLookBackMetrics>();
        ref var freezeMetric = ref playerEntity.Get<FearFreezeMetrics>();

        _observer.Get<PlayerModel>().Composure = Mathf.Clamp01(
            lightMetricComp.LightPreferencesRatio*
            lookBackMetricsComp.Frequency*
            freezeMetric.FearFreeze
            );
        //позже будут добавлятьсяи прочие метрики, влияющие на страх через сложение (lightMetricComp.LightPreferencesRatio+...)
    }
}
