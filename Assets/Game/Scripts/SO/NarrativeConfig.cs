using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NarrativeConfig", menuName = "Configs/NarrativeConfig")]
public class NarrativeConfig : ScriptableObject
{
    public List<Theme> Themes;
    public List<Command> Commands;
    public float NarrativeUpdateInterval;
    public float FreshesCommand;
}