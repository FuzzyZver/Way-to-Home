using System.Collections.Generic;
using UnityEngine;


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

[System.Serializable]
public class CharacterDialogData
{
    public CharacterType CharacterType;
    public List<DialogueGroup> Dialogues;
}

public enum CharacterType
{
    None,
    Scene,
    Mila,
    HooliganInGreyTshirt,
    HooliganInJacket,
    HooliganFemale,
    BuilderAdam,
    BuilderGosha,
    Obstacle
}

