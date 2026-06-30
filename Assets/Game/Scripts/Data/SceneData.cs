using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class SceneData : MonoBehaviour
{
    public PlayerActor Player;
    public Camera Camera;
    public NavMeshSurface NavMeshSurface;
    public InteractionActor[] Interactions;
    public List<Light> Lights;
}
