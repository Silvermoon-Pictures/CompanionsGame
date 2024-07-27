using System;
using System.Collections;
using Companions.Common;
using Companions.Systems;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class CutsceneComponent : MonoBehaviour, ICompanionComponent
{
    [SerializeField]
    private bool playOnAwake;
    [SerializeField] 
    private Collider trigger;
    [SerializeField] 
    private UnityEvent onCutsceneStarted;
    [SerializeField]
    private UnityEvent onCutsceneStopped;
    
    private PlayableDirector director;
    private Coroutine recognitionCoroutine;
    private bool hasPlayed;

    void ICompanionComponent.WorldLoaded()
    {
        director = GetComponent<PlayableDirector>();
        director.stopped += OnCutsceneEnd;

        if (playOnAwake)
            Play();
        else if (trigger != null)
            recognitionCoroutine = StartCoroutine(RecognizePlayer());
    }

    void ICompanionComponent.Cleanup()
    {
        if (recognitionCoroutine != null)
        {
            StopCoroutine(recognitionCoroutine);
            recognitionCoroutine = null;
        }
        
        if (director != null)
        {
            director.stopped -= OnCutsceneEnd;
            director = null;
        }

        hasPlayed = false;
    }

    private IEnumerator RecognizePlayer()
    {
        while (true)
        {
            if (hasPlayed)
                yield break;
            
            if (IsPlayerInColliderBounds(trigger))
            {
                Play();
                hasPlayed = true;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private void Play()
    {
        CutsceneSystem.Play(director);
        onCutsceneStarted?.Invoke();
        PlayerSystem.Player.DisableCamera();
    }

    private void OnCutsceneEnd(PlayableDirector _)
    {
        onCutsceneStopped?.Invoke();
        PlayerSystem.Player.EnableCamera();
    }
    
    private static bool IsPlayerInColliderBounds(Collider collider)
    {
        return collider.bounds.Contains(PlayerSystem.Player.transform.position);
    }
}
