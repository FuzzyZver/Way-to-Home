using System.Collections.Generic;
using UnityEngine;
using Leopotam.Ecs;

public class EcsInclude : MonoBehaviour
{

    [SerializeField] private UI _ui;
    [SerializeField] private GameConfig _gameConfig;
    [SerializeField] private SceneData _sceneData;
    private RealtimeData _realtimeData = new RealtimeData();
    private EcsWorld _world;
    private EcsSystems _systems;

    public void Awake()
    {
        _world = new EcsWorld();
        _systems = new EcsSystems(_world);

        _systems
            //Add (new ...
            .Add(new InitSystem())
            .Add(new InputSystem())
            .Add(new MovementSystem())
            .Add(new LookSystem())
            .Add(new InteractSystem())
            .Add(new FlashlightSystem())
            .Add(new FlashLightraycastSystem())
            //.Add(new EnemyFolowSystem())
            .Add(new EnemyFlashlightTrigger())
            .Add(new EnemyFindHidePointSystem())
            .Add(new TextDialogSystem())
            //.Add(new FadeSystem())

            //OneFrame<..
            .OneFrame<MoveInputEvent>()
            .OneFrame<LookInputEvent>()
            .OneFrame<ScrollInputEvent>()
            .OneFrame<InteractInputEvent>()
            .OneFrame<FlashLightRaycastEvent>()
            .OneFrame<MoveEnemyEvent>()
            .OneFrame<TextDialogEvent>()
            .OneFrame<ContinueInputEvent>()


            .Inject(_world)
            .Inject(_gameConfig)
            .Inject(_ui)
            .Inject(_sceneData)
            .Inject(_realtimeData)


            .Init();
    }

    public void Update()
    {
        _systems.Run();
    }

    public void Destroy()
    {
        _systems.Destroy();
        _world.Destroy();
    }
}