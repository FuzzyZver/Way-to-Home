using UnityEngine;
using Leopotam.Ecs;

public class ObserverSystem : Injects, IEcsInitSystem, IEcsRunSystem
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

        ref var playerModel = ref _observer.Get<PlayerModel>();

        playerModel.LightPreference = lightMetricComp.LightPreferencesRatio;
        playerModel.LookBackFrequency = lookBackMetricsComp.Frequency;
        playerModel.FearFreeze = freezeMetric.FearFreeze;

        playerModel.Composure = Mathf.Clamp01(
            playerModel.LightPreference *
            playerModel.LookBackFrequency *
            playerModel.FearFreeze
            );
        //позже будут добавляться и прочие метрики, влияющие на страх через сложение
    }
}