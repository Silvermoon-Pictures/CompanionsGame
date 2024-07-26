using System;
using Companions.Systems;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class CutsceneComponent : MonoBehaviour
{
    public UnityEvent onCutsceneStarted;
    public UnityEvent onCutsceneStopped;
    private PlayableDirector director;

    private void OnEnable()
    {
        director = GetComponent<PlayableDirector>();
        director.stopped += OnCutsceneEnd;
    }

    private void OnDisable()
    {
        director.stopped -= OnCutsceneEnd;
        director = null;
    }

    public void Play()
    {
        CutsceneSystem.Play(director);
        onCutsceneStarted?.Invoke();
    }

    private void OnCutsceneEnd(PlayableDirector _)
    {
        onCutsceneStopped?.Invoke();
    }
}
