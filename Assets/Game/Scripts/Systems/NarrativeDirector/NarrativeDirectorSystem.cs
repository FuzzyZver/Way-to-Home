using UnityEngine;
using Leopotam.Ecs;
using System.Diagnostics;

public class NarrativeDirectorSystem: Injects, IEcsInitSystem, IEcsRunSystem
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
            };
            entity.Get<CommandReadyFlag>();
        }
    }

    public void Run()
    {
        if (Time.time-_lastUpdateTime < _updateInterval) return;
        _lastUpdateTime = Time.time;

        foreach(int i in _playerModelFilter)
        {
            ref var playerModel = ref _playerModelFilter.Get1(i);
            ThemeId currentTheme = GetCurrentTheme(in playerModel);
            var currentCommand = CommandGamble(currentTheme);
            if (currentCommand == null) continue;
            currentCommand.Get<CommandOnBoardFlag>();
            currentCommand.Get<Command>().LastTimeUsed = Time.time;
        }
    }

    private ThemeId GetCurrentTheme(in PlayerModel playerModel)
    {
        float best = -1f;
        ThemeId bestTheme = ThemeId.None;

        foreach(var theme in _narrativeConfig.Themes)
        {
            float score = 1f;
            foreach(var consideration in theme.Consideration)
            {
                float param = ReadParam(in playerModel, consideration.ParamType);
                score *= consideration.ParamCurve.Evaluate(param);
            }
            if (score > best)
            {
                best = score;
                bestTheme = theme.ThemeId;
            }
        }
        GetCurrentThemeScore(bestTheme);
        return bestTheme;
    }

    private EcsEntity CommandGamble(ThemeId currentTheme)
    {
        float bestScore = -1f;
        var bestCommandEntity = EcsEntity.Null;
        foreach (int i in _commandsReadyFilter)
        {
            ref var commandComp = ref _commandsReadyFilter.Get1(i);

            float themeFit = 0.1f;
            foreach(var commandFit in commandComp.ThemeFits)
            {
                if(commandFit.ThemeId == currentTheme)
                {
                    themeFit = commandFit.Fit;
                    break;
                }
            }
            float fresh = Mathf.Clamp01((Time.time - commandComp.LastTimeUsed) / _freshes);
            float score = commandComp.Credibility * themeFit * fresh;
            commandComp.CurrentScore = Mathf.Clamp01(score);
            if (score > bestScore)
            {
               bestScore = score;
               bestCommandEntity = _commandsReadyFilter.GetEntity(i);
            }
        }
        return bestCommandEntity;
    }

    private float ReadParam(in PlayerModel playerModel, ParamType paramType)
    {
        switch (paramType)
        {
            case ParamType.Composure:
                return playerModel.Composure;
            default:
                return 0f;
        }
    }

    [Conditional("DEV_OVERLAY")]
    private void GetCurrentThemeScore(ThemeId currentTheme)
    {
        EcsWorld.NewEntity().Get<DebugEvent>() = new DebugEvent
        {
            Message = $"Current theme {currentTheme}",
            Type = DebugType.Info
        };
    }
}
