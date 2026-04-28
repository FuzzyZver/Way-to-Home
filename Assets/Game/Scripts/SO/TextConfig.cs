using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TextConfig", menuName = "Configs/TextConfig")]
public class TextConfig : ScriptableObject
{
    public TextDialogScreen TextDialogScreen;
    public float TextTypingDuration;
    public float UISlowMoveDuration;
    public float UIFastMoveDuration;

    public List<DialogueGroup> FirstCharacterDialogs;
}

[System.Serializable]
public class DialogueLine
{
    public string characterName;
    [TextArea(4, 10)]
    public string text;
}

[System.Serializable]
public class DialogueGroup
{
    public List<DialogueLine> Lines;
}
