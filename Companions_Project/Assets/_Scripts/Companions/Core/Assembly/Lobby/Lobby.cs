using System.Collections;
using Silvermoon.Core;
using UnityEngine;

public class Lobby : MonoBehaviour
{
    private IGame game;

    private void Awake()
    {
        game = GetComponent(typeof(IGame)) as IGame;
    }

    private IEnumerator Start()
    {
        yield return Initialize();
    }

    private IEnumerator Initialize()
    {
        yield return game.Initialize();
    }
}
