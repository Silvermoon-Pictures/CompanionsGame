using System;
using System.Collections;
using Companions.Common;
using Companions.Systems;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Video;

public class CutsceneComponent : MonoBehaviour, ICompanionComponent
{
    private enum ECutsceneType
    {
        Timeline,
        Video
    }
    
    [SerializeField]
    private bool playOnAwake;
    [SerializeField]
    private ECutsceneType cutsceneType;
    [SerializeField, ShowIf("@cutsceneType == ECutsceneType.Timeline")]
    private PlayableDirector director;
    [SerializeField, ShowIf("@cutsceneType == ECutsceneType.Video")]
    private VideoPlayer videoPlayer;
    [SerializeField] 
    private Collider trigger;
    [SerializeField] 
    private UnityEvent onCutsceneStarted;
    [SerializeField]
    private UnityEvent onCutsceneStopped;
    
    private Coroutine recognitionCoroutine;
    private bool hasPlayed;

    void ICompanionComponent.WorldLoaded()
    {
        if (director != null)
            director.stopped += OnCutsceneEnd;
        else if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnded;
            videoPlayer.targetCamera = CameraSystem.Camera;
        }

        if (!playOnAwake && trigger == null)
            Debug.LogError($"The Cutscene on the object {gameObject.name} will not play because {nameof(playOnAwake)} is false and collider is not set.");

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
            director.stopped -= OnCutsceneEnd;
        else if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnded;

        hasPlayed = false;
    }

    private IEnumerator RecognizePlayer()
    {
        yield return new WaitUntil(() => PlayerSystem.Player != null);
        
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
        if (director != null)
            CutsceneSystem.Play(director);
        else if (videoPlayer != null)
            CutsceneSystem.Play(videoPlayer);
        
        onCutsceneStarted?.Invoke();
        CameraSystem.DisableCamera();
    }

    private void OnCutsceneEnd(PlayableDirector _)
    {
        onCutsceneStopped?.Invoke();
        CameraSystem.EnableCamera();
    }
    
    private void OnVideoEnded(VideoPlayer _)
    {
        onCutsceneStopped?.Invoke();
        CameraSystem.EnableCamera();
    }
    
    private static bool IsPlayerInColliderBounds(Collider collider)
    {
        return collider.bounds.Contains(PlayerSystem.Player.transform.position);
    }
}
