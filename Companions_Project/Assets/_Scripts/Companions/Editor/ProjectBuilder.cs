using UnityEditor;
using UnityEngine;

public class ProjectBuilder
{
    public static void BuildWindows64()
    {
        string buildPath = "Build/Windows64";
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath + "/Companions.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        Debug.Log("Build completed successfully.");
    }
    
    public static void BuildMac()
    {
        string buildPath = "Build/Mac";
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath + "/Companions.app", BuildTarget.StandaloneOSX, BuildOptions.None);
        Debug.Log("Mac build completed successfully.");
    }
}