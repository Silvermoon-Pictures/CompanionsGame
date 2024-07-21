using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public struct SearchContextElement
{
    public object target { get; private set; }
    public string title { get; private set; }

    public SearchContextElement(object target, string title)
    {
        this.target = target;
        this.title = title;
    }
}

public class ActionGraphSearchProvider : ScriptableObject, ISearchWindowProvider
{
    public ActionGraphView graph;
    public VisualElement target;

    public static List<SearchContextElement> elements;
    
    List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> tree = new();
        tree.Add(new SearchTreeGroupEntry(new GUIContent("Nodes"), 0));

        elements = new();
        
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            foreach (Type type in assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute(typeof(NodeInfoAttribute));
                if (attribute != null)
                {
                    NodeInfoAttribute att = (NodeInfoAttribute)attribute;
                    var node = Activator.CreateInstance(type);

                    if (string.IsNullOrEmpty(att.MenuItem))
                        continue;

                    elements.Add(new SearchContextElement(node, att.MenuItem));
                }
            }
        }

        elements.Sort((entry1, entry2) =>
        {
            string[] splits1 = entry1.title.Split('/');
            string[] splits2 = entry2.title.Split('/');
            for (int i = 0; i < splits1.Length; i++)
            {
                if (i >= splits2.Length)
                    return 1;

                int value = splits1[i].CompareTo(splits2[i]);
                if (value != 0)
                {
                    if (splits1.Length != splits2.Length && (i == splits1.Length - 1 || i == splits2.Length))
                        return splits1.Length < splits2.Length ? 1 : -1;

                    return value;
                }
            }

            return 0;
        });

        List<string> groups = new();

        foreach (SearchContextElement element in elements)
        {
            string[] entryTitle = element.title.Split('/');

            string groupName = "";

            for (int i = 0; i < entryTitle.Length - 1; i++)
            {
                groupName += entryTitle[i];
                if (!groups.Contains(groupName))
                {
                    tree.Add(new SearchTreeGroupEntry(new GUIContent(entryTitle[i]), i + 1));
                    groups.Add(groupName);
                }

                groupName += "/";
            }

            SearchTreeEntry entry = new(new GUIContent(entryTitle.Last()));
            entry.level = entryTitle.Length;
            entry.userData = element;
            tree.Add(entry);
        }

        return tree;
    }

    bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
    {
        var mousePosition = graph.ChangeCoordinatesTo(graph, context.screenMousePosition - graph.Window.position.position);
        var graphMousePosition = graph.contentViewContainer.WorldToLocal(mousePosition);

        SearchContextElement element = (SearchContextElement)searchTreeEntry.userData;
        ActionGraphNode node = (ActionGraphNode)element.target;
        node.SetPosition(new Rect(graphMousePosition, new Vector2()));
        graph.Add(node);
        return true;
    }
}
