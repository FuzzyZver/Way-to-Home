using Leopotam.Ecs;
using System.Collections.Generic;
using UnityEngine;

public class TextDialogSystem: Injects, IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter<TextDialogEvent> _textDialogEventFilter;
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

        if (_currentDialogScreen)
        {
            if (_currentDialog.Lines.Count >= _dialogLine)
            {
                _dialogLine = 0;
                playerEntity.Del<FreezeFlag>();
                _currentDialogScreen = null;
            }
            else
            {
                var speech = _currentDialog.Lines[_dialogLine];
                _currentDialogScreen.Speech(speech.characterName, speech.text);
                _dialogLine++;
            }
        }

        if (playerEntity.Has<DeadFlag>()) return;
        if (playerEntity.Has<FreezeFlag>()) return;

        foreach (int i in _textDialogEventFilter)
        {
            var characterEntity = _textDialogEventFilter.Get1(i).InteractionActor.GetEntity();
            var characterTransform = characterEntity.Get<TransformRef>().Transform;
            ref var charComp = ref characterEntity.Get<CharacterComponent>();
            playerEntity.Get<CameraRef>().Camera.transform.LookAt(characterTransform);

            _currentDialogScreen = GameObject.Instantiate(_textConfig.TextDialogScreen, UI.transform);
            _currentDialogScreen.StartDialog();
            if(charComp.CharacterId == 1)
            {
                for (int j = 0; j < charComp.CompleteDialogs.Count; j++)
                {
                    if (!charComp.CompleteDialogs[j])
                    {
                        _currentDialog = _textConfig.FirstCharacterDialogs[j];
                        break;
                    }
                }
            }
            playerEntity.Get<FreezeFlag>();
        }
    }
}
