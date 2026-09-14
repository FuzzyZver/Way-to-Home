using Leopotam.Ecs;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public class MakeLightHostileExecutorSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<MakeLightHostileEvent> _hostileEventFilter;
    private EcsFilter<SegmentSlotComponent> _slotsFilter;
    private EcsFilter<HostileLightComponent, LightRef> _hostileLightFilter;

    private PlayerActor _player;
    private GameObject _shadowCaster;

    private float _defaultDuration;
    private float _shadowLateralOffset;
    private float _shadowCasterPlacement;
    private float _humVolume;
    private LayerMask _occlusionMask;
    private List<AudioClip> _humClips;

    public void Init()
    {
        _player = SceneData.Player;

        var executionConfig = GameConfig.ExecutionConfig;
        _defaultDuration = executionConfig.MakeLightHostileDuration;
        _shadowLateralOffset = executionConfig.HostileShadowLateralOffset;
        _shadowCasterPlacement = executionConfig.HostileShadowCasterPlacement;
        _humVolume = executionConfig.HostileHumVolume;
        _humClips = GameConfig.SoundConfig.LightHumSounds;
        _occlusionMask = GameConfig.PlayerMetricsConfig.RadialScanMask;

        if (executionConfig.ShadowCasterPrefab != null)
        {
            _shadowCaster = GameObject.Instantiate(executionConfig.ShadowCasterPrefab);
            _shadowCaster.SetActive(false);
        }
    }

    public void Run()
    {
        var playerEntity = _player.GetEntity();
        var playerPosition = playerEntity.Get<TransformRef>().Transform.position;

        foreach (int i in _hostileEventFilter)
        {
            ref var hostileEvent = ref _hostileEventFilter.Get1(i);
            MakeHostile(in hostileEvent, playerPosition);
        }

        TickHostileLights();
    }

    private void MakeHostile(in MakeLightHostileEvent hostileEvent, Vector3 playerPosition)
    {
        if (_hostileLightFilter.GetEntitiesCount() > 0)
        {
            ReportSkip("Another light is already hostile");
            return;
        }

        if (!TryFindNearestLight(playerPosition, out Light light))
        {
            ReportSkip("No available lights in current slot");
            return;
        }

        float duration = hostileEvent.Duration > 0f ? hostileEvent.Duration : _defaultDuration;

        var entity = EcsWorld.NewEntity();
        entity.Get<LightRef>().Light = light;

        ref var hostileComp = ref entity.Get<HostileLightComponent>();
        hostileComp.Remaining = duration;
        hostileComp.OriginalIntensity = light.intensity;
        hostileComp.OriginalShadows = light.shadows;
        hostileComp.AudioSource = StartHum(light.gameObject);

        if (light.shadows == LightShadows.None) light.shadows = LightShadows.Soft;

        entity.Get<BlinkingLightFlag>().Duration = duration;

        PlaceShadowCaster(light);

        ReportStart(light.gameObject.name, duration);
    }

    private void TickHostileLights()
    {
        foreach (int i in _hostileLightFilter)
        {
            ref var hostileComp = ref _hostileLightFilter.Get1(i);
            var light = _hostileLightFilter.Get2(i).Light;
            var entity = _hostileLightFilter.GetEntity(i);

            hostileComp.Remaining -= Time.deltaTime;
            if (hostileComp.Remaining > 0f) continue;

            Restore(entity, ref hostileComp, light);
        }
    }

    private void Restore(EcsEntity entity, ref HostileLightComponent hostileComp, Light light)
    {
        if (hostileComp.AudioSource != null)
        {
            hostileComp.AudioSource.Stop();
            hostileComp.AudioSource.loop = false;
            hostileComp.AudioSource.clip = null;
        }

        if (light != null)
        {
            light.shadows = hostileComp.OriginalShadows;
            light.enabled = true;
            light.intensity = hostileComp.OriginalIntensity;
        }

        if (_shadowCaster != null) _shadowCaster.SetActive(false);

        entity.Destroy();
        ReportStop();
    }

    private bool TryFindNearestLight(Vector3 playerPosition, out Light result)
    {
        result = null;
        float bestSqrDistance = float.MaxValue;

        foreach (int i in _slotsFilter)
        {
            ref var slot = ref _slotsFilter.Get1(i);
            if (slot.Position != SegmentRelativePosition.Current) continue;
            if (slot.Objects == null) continue;

            for (int j = 0; j < slot.Objects.Count; j++)
            {
                var slotObject = slot.Objects[j];

                if (slotObject.Type != SegmentObjectsType.Light) continue;
                if (slotObject.Object == null) continue;
                if (!slotObject.Object.activeInHierarchy) continue;
                if (!slotObject.Object.TryGetComponent(out Light light)) continue;
                if (!light.enabled) continue;

                float sqrDistance = (light.transform.position - playerPosition).sqrMagnitude;
                if (sqrDistance >= bestSqrDistance) continue;

                bestSqrDistance = sqrDistance;
                result = light;
            }
        }

        return result != null;
    }

    private AudioSource StartHum(GameObject lightObject)
    {
        if (_humClips == null || _humClips.Count == 0) return null;
        if (!lightObject.TryGetComponent(out AudioSource source)) return null;

        source.clip = _humClips[Random.Range(0, _humClips.Count)];
        source.loop = true;
        source.spatialBlend = 1f;
        source.volume = _humVolume * GameConfig.SoundConfig.SFXVolume;
        source.Play();

        return source;
    }

    /// <summary>
    /// Каст привязан к лампе, а не к игроку: гудит лампа, на неё игрок и смотрит,
    /// и тень должна лежать в её собственном пятне независимо от того, куда игрок отошёл.
    /// </summary>
    private void PlaceShadowCaster(Light light)
    {
        if (_shadowCaster == null) return;

        Vector3 lampPosition = light.transform.position;
        Vector3 lampForward = light.transform.forward;

        float beamLength = light.range;
        if (Physics.Raycast(lampPosition, lampForward, out RaycastHit hit, light.range, _occlusionMask, QueryTriggerInteraction.Ignore))
            beamLength = hit.distance;

        Vector3 casterPosition = lampPosition + lampForward * (beamLength * _shadowCasterPlacement);

        Vector3 lateral = light.transform.right;
        lateral.y = 0f;
        if (lateral.sqrMagnitude < 0.001f) lateral = Vector3.right;
        casterPosition += lateral.normalized * _shadowLateralOffset;

        _shadowCaster.transform.position = casterPosition;

        Vector3 toLamp = lampPosition - casterPosition;
        toLamp.y = 0f;
        if (toLamp.sqrMagnitude > 0.001f) _shadowCaster.transform.rotation = Quaternion.LookRotation(toLamp);

        _shadowCaster.SetActive(true);
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportStart(string lightName, float duration)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[MAKE LIGHT HOSTILE] {lightName} for {duration:0.0}s",
            Type = DebugType.Info
        };
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportStop()
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = "[MAKE LIGHT HOSTILE] Restored",
            Type = DebugType.Info
        };
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportSkip(string reason)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[MAKE LIGHT HOSTILE] Skipped: {reason}",
            Type = DebugType.Warning
        };
    }
}