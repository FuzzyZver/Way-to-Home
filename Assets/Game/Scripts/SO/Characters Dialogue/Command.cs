using UnityEngine;

[System.Serializable]
public struct Command
{
    public string Name;
    public float CurrentScore;
    public CommandType Type;
    public float Cooldown;
    public float LastTimeUsed;
    public float Credibility;
    public ThemeFit[] ThemeFits;
}

[System.Serializable]
public struct ThemeFit
{
    public ThemeId ThemeId;
    public float Fit;
}

public enum CommandType
{
    None,
    LightOff
    //....
    //type100n
}
