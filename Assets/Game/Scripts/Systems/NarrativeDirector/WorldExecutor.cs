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
                EcsWorld.NewEntity().Get<LightOffEvent>() = new LightOffEvent
                {
                    Duration = 10f,
                    Count = 2
                };
                break;
            case CommandType.FootstepsBehind:
                EcsWorld.NewEntity().Get<FootstepsBehindEvent>() = new FootstepsBehindEvent
                {
                    Duration = 10f
                };
                break;
            case CommandType.StalkerGlimpse:
                EcsWorld.NewEntity().Get<StalkerGlimpseEvent>() = new StalkerGlimpseEvent
                {
                    Duration = 0f,
                    SlotCount = 4
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
