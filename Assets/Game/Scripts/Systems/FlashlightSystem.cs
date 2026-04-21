using Leopotam.Ecs;
using UnityEngine;

public class FlashlightSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<ScrollInputEvent> _scrollInputEventFilter;
    private PlayerConfig _playerConfig;
    private PlayerActor _playerRef;
    private Light _flashlight;
    


    public void Init()
    {
        _playerRef = SceneData.Player;
        _flashlight = _playerRef.GetEntity().Get<FlashlightComponent>().Light;
        _playerConfig = GameConfig.PlayerConfig;
    }

    public void Run()
    {
        var playerEntity = _playerRef.GetEntity();
        if (playerEntity.Has<DeadFlag>()) return;
        if (playerEntity.Has<FreezeFlag>()) return;

        var flashlightTransform = _flashlight?.transform;
        flashlightTransform.rotation = playerEntity.Get<CameraTargetRef>().Transform.rotation;

        foreach (int i in _scrollInputEventFilter)
        {
            ref var eventComp = ref _scrollInputEventFilter.Get1(i);
            float flashlightZoom = eventComp.Value * _playerConfig.ScrollSpeed;
            _flashlight.range = Mathf.Clamp(_flashlight.range + flashlightZoom, _playerConfig.FlashlightMinRange, _playerConfig.FlashlightMaxRange);

            _flashlight.spotAngle = _playerConfig.SpotAngel * 3/ _flashlight.range;
            _flashlight.innerSpotAngle = _playerConfig.SpotAngel * 2 / _flashlight.range;

            _flashlight.intensity = _playerConfig.Intensity * _flashlight.range;
        }

    }
}
