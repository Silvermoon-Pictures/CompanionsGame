using System;
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
    public UnityEvent onCutsceneStarted;
    public UnityEvent onCutsceneStopped;
    private PlayableDirector director;

    void ICompanionComponent.WorldLoaded()
    {
        director = GetComponent<PlayableDirector>();
        director.stopped += OnCutsceneEnd;

        if (playOnAwake)
            Play();
    }

    void ICompanionComponent.Cleanup()
    {
        if (director != null)
        {
            director.stopped -= OnCutsceneEnd;
            director = null;
        }
    }

    public void Play()
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
}
