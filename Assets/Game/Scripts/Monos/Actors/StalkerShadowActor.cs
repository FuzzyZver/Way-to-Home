using UnityEngine;
using Leopotam.Ecs;

public class StalkerShadowActor : Actor
{
    [SerializeField] private Transform _transform;
    [SerializeField] private Renderer _renderer;

    public override void ExpandEntity(EcsEntity entity)
    {
        entity.Get<TransformRef>().Transform = _transform;
        entity.Get<RendererRef>().Renderer = _renderer;
        entity.Get<StalkerShadowRef>().Actor = this;
    }

    /// <summary>
    /// Материал переживает возврат в пул, поэтому растворённую тень
    /// перед повторной выдачей нужно вернуть в видимое состояние.
    /// </summary>
    public void ResetDissolve()
    {
        if (_renderer == null) return;

        var material = _renderer.material;
        if (material.HasProperty("_DissolveAmount")) material.SetFloat("_DissolveAmount", 0f);
    }
}