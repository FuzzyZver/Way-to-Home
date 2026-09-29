using UnityEngine;

public class WrongDetailView : MonoBehaviour
{
    [SerializeField] private GameObject _normalState;
    [SerializeField] private GameObject _wrongState;

    public GameObject NormalState => _normalState;
    public GameObject WrongState => _wrongState;

    public bool IsValid => _normalState != null && _wrongState != null;
}