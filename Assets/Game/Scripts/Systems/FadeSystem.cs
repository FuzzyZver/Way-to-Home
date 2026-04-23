using Leopotam.Ecs;
using UnityEngine;

public class FadeSystem : Injects, IEcsRunSystem
{
    private EcsFilter<FadeComponent, RendererRef> _filter;

    public void Run()
    {
        foreach (int i in _filter)
        {
            ref var fadeComp = ref _filter.Get1(i);
            var renderer = _filter.Get2(i).Renderer;

            fadeComp.Current = Mathf.MoveTowards(
                fadeComp.Current,
                fadeComp.Target,
                fadeComp.Speed * Time.deltaTime
            );

            var mat = renderer.material;

            if (mat.HasProperty("_DissolveAmount"))
            {
                mat.SetFloat("_DissolveAmount", 1f - fadeComp.Current);
            }

            if (Mathf.Abs(fadeComp.Current - fadeComp.Target) < 0.01f)
            {
                var entity = _filter.GetEntity(i);

                entity.Del<FadeComponent>();

                if (fadeComp.Target <= 0f)
                {
                    entity.Get<DespawnFlag>();
                }
            }
        }
    }
}
