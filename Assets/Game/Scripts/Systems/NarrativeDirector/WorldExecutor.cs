using UnityEngine;
using Leopotam.Ecs;

public class WorldExecutor: Injects, IEcsRunSystem
{
    private EcsFilter<Command, CommandOnBoardFlag> _commandOnBoardFlagFilter;
    private EcsFilter<Command, CommandActiveFlag> _commandActiveFlagFilter;

    public void Run()
    {
        foreach(int i in _commandOnBoardFlagFilter)
        {
            var commandEntity = _commandOnBoardFlagFilter.GetEntity(i);
            ref var command = ref _commandOnBoardFlagFilter.Get1(i);
            commandEntity.Del<CommandOnBoardFlag>();
            commandEntity.Get<CommandActiveFlag>();
            ExecuteCommand(in command);
        }

        foreach (int i in _commandActiveFlagFilter)
        {
            ref var command = ref _commandActiveFlagFilter.Get1(i);
            if (Time.time - command.LastTimeUsed > command.Cooldown)
            {
                var commandEntity = _commandActiveFlagFilter.GetEntity(i);
                commandEntity.Del<CommandActiveFlag>();
                commandEntity.Get<CommandReadyFlag>();
            }
        }
    }

    private void ExecuteCommand(in Command command)
    {
        switch (command.Type)
        {
            case CommandType.None:
                EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
                {
                    Message = $"[WORLD EXECUTOR] The command type has not been specified. Go to config and specify the type.",
                    Type = DebugType.Warning
                };
                break;
            case CommandType.LightOff:
                EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
                {
                    Message = $"[WORLD EXECUTOR] Executing command {command.Type}",
                    Type = DebugType.Info
                };

                int probability = Random.Range(0, 3);
                var entity = EcsWorld.NewEntity();
                var gameObject = SceneData.Lights[probability].gameObject;
                gameObject.SetActive( false );
                entity.Get<ActivatesFlag>() = new ActivatesFlag
                {
                    Duration = command.Cooldown,
                    GameObject = gameObject
                };
                break;
            default:
                EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
                {
                    Message = $"[WORLD EXECUTOR] Unknown command: {command.Type}. " +
                    $"Please, change CommandType in config or add logic for this {command.Type}",
                    Type = DebugType.Error
                };
                break;
        }
    }
}
