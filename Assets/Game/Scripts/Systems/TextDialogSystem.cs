using Leopotam.Ecs;
using UnityEngine;

public class TextDialogSystem : Injects, IEcsInitSystem, IEcsRunSystem
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
                    playerEntity.Del<CameraFocusFlag>();
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
            var characterEntity = _textDialogEventFilter.Get1(i).CharacterEntity;
            var characterCameraTarget = characterEntity.Get<CameraTargetRef>().Transform;
            ref var charComp = ref characterEntity.Get<CharacterComponent>();

            var characterDialogData = _textConfig.Characters.Find(data => data.CharacterType == characterEntity.Get<CharacterComponent>().CharacterType);

            if (characterDialogData == null) continue;

            for (int j = 0; j < characterDialogData.Dialogues.Count; j++)
            {
                if (!charComp.CompletedDialogs.Contains(j))
                {
                    _currentDialog = characterDialogData.Dialogues[j];
                    _currentDialogScreen = GameObject.Instantiate(_textConfig.TextDialogScreen, UI.transform);
                    _currentDialogScreen.StartDialog();
                    playerEntity.Get<FreezeFlag>();
                    playerEntity.Get<CameraFocusFlag>().Transform = characterCameraTarget;
                    Dialog();
                    charComp.CompletedDialogs.Add(j);
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