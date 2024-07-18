using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DictionaryEntry
{
    public Identifier key;
    public Object value;
}

public class DictionaryComponent : MonoBehaviour
{
    public List<DictionaryEntry> dictionaryEntries;
    
    public T Get<T>(Identifier key)
    {
        foreach (var entry in dictionaryEntries)
        {
            if (entry.key == key && entry.value is T obj)
                return obj;
        }

        return default;
    }
}
