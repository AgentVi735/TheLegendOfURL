using System.IO;
using UnityEditor;
using UnityEngine;

public class ScreenshotEditorWindow : EditorWindow
{
    private string savePath = "Screenshots/Screenshot.png";
    private Camera targetCamera;
    private int width = 3840;
    private int height = 2160;
    private string transparentSavePath = "Screenshots/Transparent.png";

    [MenuItem("Tools/Game View Screenshot")]
    public static void ShowWindow()
    {
        GetWindow<ScreenshotEditorWindow>("Screenshot");
    }

    private void OnGUI()
    {
        GUILayout.Label("Normal Screenshot", EditorStyles.boldLabel);

        savePath = EditorGUILayout.TextField("Save Path", savePath);

        GUILayout.Space(18);

        if (GUILayout.Button("Capture Game View"))
            CaptureScreenshot();
        
        GUILayout.Space(36);
        
        GUILayout.Label("Transparent Screenshot", EditorStyles.boldLabel);

        targetCamera = (Camera)EditorGUILayout.ObjectField(
            "Camera",
            targetCamera,
            typeof(Camera),
            true
        );

        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);

        transparentSavePath = EditorGUILayout.TextField("Save Path", transparentSavePath);

        GUILayout.Space(18);

        if (GUILayout.Button("Capture Transparent Screenshot"))
            CaptureTransparentScreenshot();
    }

    private void CaptureScreenshot()
    {
        string directory = Path.GetDirectoryName(savePath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        ScreenCapture.CaptureScreenshot(savePath);

        Debug.Log($"Screenshot captured to: {Path.GetFullPath(savePath)}");
    }
    
    private void CaptureTransparentScreenshot()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogError("No camera assigned and no Main Camera found.");
            return;
        }

        RenderTexture renderTexture = new RenderTexture(
            width,
            height,
            24,
            RenderTextureFormat.ARGB32
        );

        renderTexture.antiAliasing = 1;

        RenderTexture previousTarget = targetCamera.targetTexture;
        CameraClearFlags previousClearFlags = targetCamera.clearFlags;
        Color previousBackgroundColor = targetCamera.backgroundColor;

        targetCamera.clearFlags = CameraClearFlags.SolidColor;
        targetCamera.backgroundColor = new Color(0, 0, 0, 0);
        targetCamera.targetTexture = renderTexture;

        targetCamera.Render();

        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D screenshot = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false
        );

        screenshot.ReadPixels(
            new Rect(0, 0, width, height),
            0,
            0
        );

        screenshot.Apply();

        targetCamera.targetTexture = previousTarget;
        targetCamera.clearFlags = previousClearFlags;
        targetCamera.backgroundColor = previousBackgroundColor;

        RenderTexture.active = previousActive;

        string directory = Path.GetDirectoryName(transparentSavePath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        byte[] pngData = screenshot.EncodeToPNG();

        File.WriteAllBytes(transparentSavePath, pngData);

        DestroyImmediate(screenshot);
        renderTexture.Release();
        DestroyImmediate(renderTexture);

        AssetDatabase.Refresh();

        Debug.Log($"Transparent screenshot saved to: {Path.GetFullPath(transparentSavePath)}");
    }
}
