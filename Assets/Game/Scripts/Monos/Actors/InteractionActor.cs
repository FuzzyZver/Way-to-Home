using Leopotam.Ecs;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.Searcher.AnalyticsEvent;

public class InteractionActor: Actor
{
    [SerializeField] private ActorType _actorType;

    [SerializeField] private Transform _transform;
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private CharacterType _characterType;

    [SerializeField] private EventType _eventType;
    [SerializeField] private AudioClip _audioClip;
    [SerializeField] private int _specialEventId;

    public CharacterType CharacterType => _characterType;
    public ActorType ActorType => _actorType;

    public override void ExpandEntity(EcsEntity entity)
    {
        entity.Get<TransformRef>().Transform = _transform;
        entity.Get<CameraTargetRef>().Transform = _cameraTarget;
        if (_eventType == EventType.TextDialogue)
            entity.Get<CharacterComponent>() = new CharacterComponent
            {
                CharacterType = _characterType,
                CompletedDialogs = new HashSet<int>()
            };
    }

    public void Interact()
    {
        if (_actorType == ActorType.Interaction)
        {
            Feedback();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_actorType == ActorType.Trigger && other.TryGetComponent<PlayerActor>(out PlayerActor player))
        {
            Feedback();
        }
    }
    
    private void Feedback()
    {
        if (_eventType == EventType.TextDialogue)
        {
            GetWorld().NewEntity().Get<TextDialogEvent>() = new TextDialogEvent
            {
                CharacterEntity = GetEntity()
            };
        }
        else if (_eventType == EventType.AudioClip)
        {
            GetWorld().NewEntity().Get<AudioEffectEvent>() = new AudioEffectEvent
            {
                AudioClip = _audioClip,
                SoundPosition = _transform.position
            };
        }
        else if (_eventType == EventType.SpecialEvent)
        {
            //GetWorld().NewEntity().Get<SpecialEvent>() = new SpecialEvent
        }
    }
}

public enum EventType
{
    TextDialogue,
    AudioClip,
    SpecialEvent
}

public enum ActorType
{
    Interaction,
    Trigger
}

