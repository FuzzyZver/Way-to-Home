using UnityEngine;

public struct DebugEvent
{
    public string Message;
    public DebugType Type;
}

public enum DebugType
{
    Info,
    Warning,
    Error
}