using UnityEngine;

public class Item : MonoBehaviour
{
    public void OnAddedToInventory()
    {
        gameObject.SetActive(false);
    }

    public void OnRemovedFromInventory()
    {
        gameObject.SetActive(true);
    }
}
