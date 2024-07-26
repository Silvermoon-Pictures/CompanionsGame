using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    private Player player;

    private void OnEnable()
    {
        player = GetComponentInParent<Player>();
    }

    public void OnLiftEvent()
    {
        Debug.Log("AnimationEventHandler::OnLiftEvent");
        player.AttachLiftable();
    }

    public void OnDropEvent()
    {
        Debug.Log("AnimationEventHandler::OnDropEvent");
        player.DetachLiftable();
    }

}
