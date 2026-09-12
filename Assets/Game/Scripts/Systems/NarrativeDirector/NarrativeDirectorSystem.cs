using UnityEngine;
using Leopotam.Ecs;
using System.Diagnostics;

public class NarrativeDirectorSystem : Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<PlayerModel> _playerModelFilter;
    private EcsFilter<Command, CommandReadyFlag> _commandsReadyFilter;
    private NarrativeConfig _narrativeConfig;
    private float _lastUpdateTime;
    private float _updateInterval;
    private float _freshes;

    public void Init()
    {
        _narrativeConfig = GameConfig.NarrativeConfig;
        _updateInterval = _narrativeConfig.NarrativeUpdateInterval;
        _freshes = _narrativeConfig.FreshesCommand;

        for (int id = 0; id < _narrativeConfig.Commands.Count; id++)
        {
            EcsEntity entity = EcsWorld.NewEntity();
            entity.Get<Command>() = new Command
            {
                Name = _narrativeConfig.Commands[id].Name,
                CurrentScore = _narrativeConfig.Commands[id].CurrentScore,
                Type = _narrativeConfig.Commands[id].Type,
                Cooldown = _narrativeConfig.Commands[id].Cooldown,
                LastTimeUsed = _narrativeConfig.Commands[id].LastTimeUsed,
                Credibility = _narrativeConfig.Commands[id].Credibility,
                ThemeFits = _narrativeConfig.Commands[id].ThemeFits,
                Considerations = _narrativeConfig.Commands[id].Considerations,
            };
            entity.Get<CommandReadyFlag>();
        }
    }

    public void Run()
    {
        if (Time.time - _lastUpdateTime < _updateInterval) return;
        _lastUpdateTime = Time.time;

        foreach (int i in _playerModelFilter)
        {
            ref var playerModel = ref _playerModelFilter.Get1(i);

            ThemeId currentTheme = GetCurrentTheme(in playerModel);
            var currentCommand = CommandGamble(in playerModel, currentTheme);
            if (currentCommand == EcsEntity.Null) continue;

            currentCommand.Get<CommandOnBoardFlag>();
            currentCommand.Get<Command>().LastTimeUsed = Time.time;
            currentCommand.Del<CommandReadyFlag>();
        }
    }

    private ThemeId GetCurrentTheme(in PlayerModel playerModel)
    {
        float best = -1f;
        ThemeId bestTheme = ThemeId.None;

        foreach (var theme in _narrativeConfig.Themes)
        {
            float score = EvaluateConsiderations(in playerModel, theme.Consideration);
            if (score > best)
            {
                best = score;
                bestTheme = theme.ThemeId;
            }
        }

        ReportTheme(bestTheme, best);
        return bestTheme;
    }

    private EcsEntity CommandGamble(in PlayerModel playerModel, ThemeId currentTheme)
    {
        float bestScore = -1f;
        var bestCommandEntity = EcsEntity.Null;

        foreach (int i in _commandsReadyFilter)
        {
            ref var commandComp = ref _commandsReadyFilter.Get1(i);

            float themeFit = 0.1f;
            foreach (var commandFit in commandComp.ThemeFits)
            {
                if (commandFit.ThemeId == currentTheme)
                {
                    themeFit = commandFit.Fit;
                    break;
                }
            }

            float fresh = Mathf.Clamp01((Time.time - commandComp.LastTimeUsed) / _freshes);

            // Насколько команда уместна именно под текущее поведение игрока.
            float relevance = EvaluateConsiderations(in playerModel, commandComp.Considerations);

            float score = commandComp.Credibility * themeFit * fresh * relevance;
            commandComp.CurrentScore = Mathf.Clamp01(score);

            if (score > bestScore)
            {
                bestScore = score;
                bestCommandEntity = _commandsReadyFilter.GetEntity(i);
            }
        }

        ReportCommandChoice(bestCommandEntity, bestScore);
        return bestCommandEntity;
    }

    private float EvaluateConsiderations(in PlayerModel playerModel, Consideration[] considerations)
    {
        if (considerations == null || considerations.Length == 0) return 1f;

        float score = 1f;
        for (int i = 0; i < considerations.Length; i++)
        {
            float param = ReadParam(in playerModel, considerations[i].ParamType);
            score *= considerations[i].ParamCurve.Evaluate(param);
        }

        return score;
    }

    private float ReadParam(in PlayerModel playerModel, ParamType paramType)
    {
        switch (paramType)
        {
            case ParamType.Composure:
                return playerModel.Composure;
            case ParamType.LightPreference:
                return playerModel.LightPreference;
            case ParamType.LookBackFrequency:
                return playerModel.LookBackFrequency;
            case ParamType.FearFreeze:
                return playerModel.FearFreeze;
            default:
                return 0f;
        }
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportTheme(ThemeId currentTheme, float score)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[DIRECTOR] Theme: {currentTheme} ({score:0.000})",
            Type = DebugType.Info
        };
    }

    [Conditional("DEV_OVERLAY")]
    private void ReportCommandChoice(EcsEntity commandEntity, float score)
    {
        if (commandEntity == EcsEntity.Null)
        {
            EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
            {
                Message = "[DIRECTOR] No command available",
                Type = DebugType.Warning
            };
            return;
        }

        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"[DIRECTOR] Command: {commandEntity.Get<Command>().Name} ({score:0.000})",
            Type = DebugType.Info
        };
    }
}