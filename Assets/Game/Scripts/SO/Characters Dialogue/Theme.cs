using UnityEngine;

[System.Serializable]
public class Theme
{
    public ThemeId ThemeId;
    public Consideration[] Consideration;
}

[System.Serializable]
public class Consideration
{
    public ParamType ParamType;
    public AnimationCurve ParamCurve;
}

public enum ThemeId { None, Tension }
public enum ParamType { Composure }
