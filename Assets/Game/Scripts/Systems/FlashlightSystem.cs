using Leopotam.Ecs;
using UnityEngine;

public class FlashlightSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<ScrollInputEvent> _scrollInputEventFilter;
    private EcsFilter<FlashlightInputEvent> _flashlightInputEventFilter;
    private PlayerConfig _playerConfig;
    private PlayerActor _playerRef;
    private Light _flashlight;
    private float _chargeLostPer;
    private float _chargeTimer;
    private float _dischargeTime;

    public void Init()
    {
        _playerRef = SceneData.Player;
        _flashlight = _playerRef.GetEntity().Get<LightRef>().Light;
        _playerConfig = GameConfig.PlayerConfig;

        _playerRef.GetEntity().Get<FlashlightChargeComponent>().Charge = _playerConfig.Charge;
        _chargeLostPer = _playerConfig.ChargeLostPer;
        _dischargeTime = _playerConfig.DischargeTime;
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

            _flashlight.spotAngle = _playerConfig.SpotAngel * 3 / _flashlight.range;
            _flashlight.innerSpotAngle = _playerConfig.SpotAngel * 2 / _flashlight.range;
        }

        float intensityMultiplier = playerEntity.Has<FlashlightModifierComponent>()
        ? playerEntity.Get<FlashlightModifierComponent>().IntensityMultiplier
        : 1f;
        _flashlight.intensity = _playerConfig.Intensity * _flashlight.range * intensityMultiplier;

        foreach (int i in _flashlightInputEventFilter)
        {
            if (!playerEntity.Has<FlashlightDischargedFlag>())
            {
                _flashlight.enabled = !_flashlight.enabled;
            }
        }

        if (_flashlight.enabled && !playerEntity.Has<FlashlightDischargedFlag>())
        {
            _chargeTimer += Time.deltaTime;

            if (_chargeTimer >= _dischargeTime)
            {
                _chargeTimer -= _dischargeTime;
                playerEntity.Get<FlashlightChargeComponent>().Charge -= _chargeLostPer;
                if(playerEntity.Get<FlashlightChargeComponent>().Charge <= 0)
                {
                    playerEntity.Get<FlashlightChargeComponent>().Charge = 0;
                    playerEntity.Get<FlashlightDischargedFlag>();
                    _flashlight.enabled = false;
                }
            }
        }
    }
}
