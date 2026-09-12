using UnityEngine;
using Leopotam.Ecs;
using System.Collections.Generic;
using System.Diagnostics;

public class LightOffExecutorSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private struct LightRestoreEntry
    {
        public Light Light;
        public float Remaining;
    }

    private EcsFilter<LightOffEvent> _lightOffEventFilter;
    private EcsFilter<SegmentSlotComponent> _slotsFilter;

    private float _defaultDuration;
    private int _maxLightsPerCommand;

    private readonly List<Light> _slotLights = new List<Light>();
    private readonly List<LightRestoreEntry> _restoring = new List<LightRestoreEntry>();

    public void Init()
    {
        _defaultDuration = GameConfig.ExecutionConfig.LightOffDuration;
        _maxLightsPerCommand = GameConfig.ExecutionConfig.MaxLightsPerCommand;
    }

    public void Run()
    {
        TickRestore();

        foreach (int i in _lightOffEventFilter)
        {
            ref var lightOffEvent = ref _lightOffEventFilter.Get1(i);
            ExecuteLightOff(in lightOffEvent);
        }
    }

    private void TickRestore()
    {
        for (int i = _restoring.Count - 1; i >= 0; i--)
        {
            var entry = _restoring[i];

            if (entry.Light == null)
            {
                _restoring.RemoveAt(i);
                continue;
            }

            entry.Remaining -= Time.deltaTime;

            if (entry.Remaining > 0f)
            {
                _restoring[i] = entry;
                continue;
            }

            entry.Light.enabled = true;
            _restoring.RemoveAt(i);
        }
    }

    private void ExecuteLightOff(in LightOffEvent lightOffEvent)
    {
        if (!TryCollectCurrentSlotLights())
        {
            ReportSkip("No available lights in current slot");
            return;
        }

        float duration = lightOffEvent.Duration > 0f ? lightOffEvent.Duration : _defaultDuration;

        int requested = lightOffEvent.Count > 0 ? lightOffEvent.Count : _maxLightsPerCommand;
        if (_maxLightsPerCommand > 0) requested = Mathf.Min(requested, _maxLightsPerCommand);
        int toDisable = Mathf.Clamp(requested, 1, _slotLights.Count);

        for (int i = 0; i < toDisable; i++)
        {
            int pick = Random.Range(i, _slotLights.Count);
            (_slotLights[i], _slotLights[pick]) = (_slotLights[pick], _slotLights[i]);

            Light light = _slotLights[i];
            light.enabled = false;

            _restoring.Add(new LightRestoreEntry
            {
                Light = light,
                Remaining = duration
            });
        }

        ReportExecution(toDisable, duration);
    }
    private bool TryCollectCurrentSlotLights()
    {
        _slotLights.Clear();

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

                _slotLights.Add(light);
            }
        }

        return _slotLights.Count > 0;
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportExecution(int count, float duration)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[LIGHT OFF] Disabled {count} light(s) for {duration:0.0}s",
            Type = DebugType.Info
        };
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportSkip(string reason)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[LIGHT OFF] Skipped: {reason}",
            Type = DebugType.Warning
        };
    }
}