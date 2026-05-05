using Leopotam.Ecs;
using UnityEngine;

public class StepSoundSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private readonly EcsFilter<MoveFlag, TransformRef> _stepSoundFilter;

    private SoundStepConfig _stepSoundConfig;

    private Vector3 _lastPosition;
    private float _passedDistance;

    public void Init()
    {
        _stepSoundConfig = GameConfig.SoundStepConfig;
        _stepSoundConfig.Inits();
    }

    public void Run()
    {
        foreach (var i in _stepSoundFilter)
        {
            ref var transformRef = ref _stepSoundFilter.Get2(i);
            var pos = new Vector3(transformRef.Transform.position.x, 0, transformRef.Transform.position.z);

            _passedDistance += Vector3.Distance(_lastPosition, pos);
            _lastPosition = pos;

            if (_passedDistance >= _stepSoundConfig.DistanceForStep)
            {
                _passedDistance = 0;

                // Проверка на землю
                if (Physics.Raycast(transformRef.Transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 1.2f))
                {
                    var mat = hit.collider.sharedMaterial;
                    if (mat != null)
                    {
                        var clip = _stepSoundConfig.GetRandomClip(mat);
                        if (clip != null)
                        {
                            EcsWorld.NewEntity().Get<AudioEffectEvent>() = new AudioEffectEvent
                            {
                                AudioClip = clip,
                                SoundPosition = pos
                            };
                        }
                    }
                }
            }
        }
    }
}
