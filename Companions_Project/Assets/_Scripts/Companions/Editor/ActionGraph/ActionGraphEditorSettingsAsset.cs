using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ActionGraphEditorSettingsAsset : ScriptableObject
{
    public Color blackboardTitleColor = Color.red;
    public int blackboardTitleSize = 32;
}

public class ActionGraphEditorSettings : SettingsProvider
{
    const string SETTINGS_ASSET_PATH = "Assets/Editor/ActionGraphEditorSettings.asset";
    const string SETTINGS_PATH = "Companions/Action Graph Settings";
    
    public static ActionGraphEditorSettingsAsset Settings => AssetDatabase.LoadAssetAtPath<ActionGraphEditorSettingsAsset>(SETTINGS_ASSET_PATH);

    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        base.OnActivate(searchContext, rootElement);
        var asset = AssetDatabase.LoadAssetAtPath<ActionGraphEditorSettingsAsset>(SETTINGS_ASSET_PATH);
        var editor = Editor.CreateEditor(asset);
        rootElement.Add(editor.CreateInspectorGUI());
    }
    
    [SettingsProvider] 
    private static SettingsProvider CreateMyCustomSettingsProvider()
    {
        if (!IsSettingsAvailable())
        {
            var settings = ScriptableObject.CreateInstance<ActionGraphEditorSettingsAsset>();
            AssetDatabase.CreateAsset(settings, SETTINGS_ASSET_PATH);
        }
        
        return new ActionGraphEditorSettings(SETTINGS_PATH, SettingsScope.Project); 
    }
    
    private static bool IsSettingsAvailable()
    {
        return System.IO.File.Exists(SETTINGS_ASSET_PATH);
    }

    public ActionGraphEditorSettings(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords) { }
}

[CustomEditor(typeof(ActionGraphEditorSettingsAsset))]
public class ActionGraphEditorSettingsEditor : Editor
{
    
    public override VisualElement CreateInspectorGUI()
    {
        GroupBox box = new GroupBox("Action Graph Settings");
        ColorField blackboardTitleColorField = new ColorField("Blackboard Title Color");
        blackboardTitleColorField.bindingPath = nameof(ActionGraphEditorSettingsAsset.blackboardTitleColor);
        blackboardTitleColorField.Bind(serializedObject);

        IntegerField blackboardTitleSize = new("Blackboard Title Size");
        blackboardTitleSize.bindingPath = nameof(ActionGraphEditorSettingsAsset.blackboardTitleSize);
        blackboardTitleSize.Bind(serializedObject);

        box.Add(blackboardTitleColorField);
        box.Add(blackboardTitleSize);
        return box;
    }
}