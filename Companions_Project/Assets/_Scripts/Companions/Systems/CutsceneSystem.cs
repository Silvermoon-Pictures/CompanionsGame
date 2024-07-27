using System;
using System.Runtime.CompilerServices;
using Silvermoon.Core;
using UnityEngine;
using UnityEngine.Playables;

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
    
    protected override void Initialize(GameContext context)
    {
        base.Initialize(context);
        
        
    }

    public static void Play(PlayableDirector director)
    {
        Instance.cutsceneStarted?.Invoke(director, EventArgs.Empty);
        director.Play();
        director.stopped += OnCutsceneEnded;
    }

    private static void OnCutsceneEnded(PlayableDirector director)
    {
        director.stopped -= OnCutsceneEnded;
        Instance.cutsceneStopped?.Invoke(director, EventArgs.Empty);

    }
}
