using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using Companions.Systems;

[RequireComponent(typeof(PlayableDirector))]
public class PlayableDirectorManager : MonoBehaviour
{
    private PlayableDirector m_director;

    private IEnumerator Start()
    {
        m_director = this.GetComponent<PlayableDirector>();

        yield return new WaitUntil(() => CameraSystem.HasBrain);

        foreach (var binding in m_director.playableAsset.outputs)
        {
            if (binding.outputTargetType == typeof(Cinemachine.CinemachineBrain))
            {
                m_director.SetGenericBinding(binding.sourceObject, CameraSystem.Brain);
            }
        }
    }
}