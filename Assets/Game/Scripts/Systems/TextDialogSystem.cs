using Leopotam.Ecs;
using System.Collections.Generic;
using UnityEngine;

public class TextDialogSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<TextDialogEvent> _textDialogEventFilter;
    private EcsFilter<ContinueInputEvent> _continueInputEventFilter;
    private TextConfig _textConfig;
    private PlayerActor _playerRef;
    private TextDialogScreen _currentDialogScreen;
    private DialogueGroup _currentDialog;
    private int _dialogLine = 0;

    public void Init()
    {
        _textConfig = GameConfig.TextConfig;
        _playerRef = SceneData.Player;
    }

    public void Run()
    {
        var playerEntity = _playerRef.GetEntity();
        if (playerEntity.Has<DeadFlag>()) return;

        foreach (int i in _continueInputEventFilter)
        {
            if (_currentDialogScreen)
            {
                if (_dialogLine >= _currentDialog.Lines.Count)
                {
                    _dialogLine = 0;
                    playerEntity.Del<FreezeFlag>();
                    _currentDialogScreen.EndDialog();
                    _currentDialogScreen = null;
                }
                else
                {
                    Dialog();
                }
            }
        }

        if (playerEntity.Has<FreezeFlag>()) return;

        foreach (int i in _textDialogEventFilter)
        {
            var characterEntity = _textDialogEventFilter.Get1(i).InteractionActor.GetEntity();
            var characterTransform = characterEntity.Get<TransformRef>().Transform;
            ref var charComp = ref characterEntity.Get<CharacterComponent>();

            for (int j = 0; j < charComp.CompleteDialogs.Count; j++)
            {
                if (!charComp.CompleteDialogs[j])
                {
                    if (charComp.CharacterId == 1)
                    {
                        _currentDialog = _textConfig.FirstCharacterDialogs[j];
                    }
                    _currentDialogScreen = GameObject.Instantiate(_textConfig.TextDialogScreen, UI.transform);
                    _currentDialogScreen.StartDialog();
                    playerEntity.Get<FreezeFlag>();
                    Dialog();
                    charComp.CompleteDialogs[j] = true;
                    break;
                }
            }
        }
    }

    private void Dialog()
    {
        var speech = _currentDialog.Lines[_dialogLine];
        _currentDialogScreen.Speech(speech.characterName, speech.text);
        _dialogLine++;
    }
}
