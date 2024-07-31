using System;
using System.Runtime.CompilerServices;
using Silvermoon.Core;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

[RequiredSystem]
public class CutsceneSystem : BaseSystem<CutsceneSystem>
{
    public static event EventHandler CutsceneStarted
    {
        add => Instance.cutsceneStarted += value;
        remove => Instance.cutsceneStarted -= value;
    }
    private event EventHandler cutsceneStarted;
    
    public static event EventHandler CutsceneStopped
    {
        add => Instance.cutsceneStopped += value;
        remove => Instance.cutsceneStopped -= value;
    }

    private event EventHandler cutsceneStopped;

    public static void Play(PlayableDirector director)
    {
        Instance.cutsceneStarted?.Invoke(Instance, EventArgs.Empty);
        director.Play();
        director.stopped += OnCutsceneEnded;
    }
    
    public static void Play(VideoPlayer videoPlayer)
    {
        Instance.cutsceneStarted?.Invoke(Instance, EventArgs.Empty);
        videoPlayer.Play();
        videoPlayer.loopPointReached += OnVideoEnded;
    }

    private static void OnCutsceneEnded(PlayableDirector director)
    {
        director.stopped -= OnCutsceneEnded;
        Instance.cutsceneStopped?.Invoke(director, EventArgs.Empty);

    }
    
    private static void OnVideoEnded(VideoPlayer videoPlayer)
    {
        videoPlayer.loopPointReached -= OnVideoEnded;
        Instance.cutsceneStopped?.Invoke(Instance, EventArgs.Empty);

    }
}
