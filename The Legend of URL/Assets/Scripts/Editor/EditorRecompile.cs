using UnityEditor;

internal static class EditorRecompile
{
    [MenuItem("Tools/Recompile")]
    private static void Recompile() => AssetDatabase.Refresh();
}