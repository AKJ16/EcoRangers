using UnityEngine;
using UnityEngine.Profiling;

public class PerformanceMonitor : MonoBehaviour
{
    private float deltaTime = 0.0f;
    private float msec;
    private float fps;
    private float allocatedMemory;

    private void Update()
    {
        // Calculate CPU Frame Time and FPS (Frames Per Second)
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        msec = deltaTime * 1000.0f;
        fps = 1.0f / deltaTime;

        // Calculate Allocated RAM in Megabytes (MB)
        long totalMemory = Profiler.GetTotalAllocatedMemoryLong();
        allocatedMemory = totalMemory / (1024f * 1024f);
    }

    private void OnGUI()
    {
        int w = Screen.width;
        int h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(15, 12, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = Color.green;

        string text = string.Format("FPS: {0:0.} | CPU Time: {1:0.0} ms | RAM: {2:0.0} MB", fps, msec, allocatedMemory);

        GUI.Box(new Rect(5, 5, 480, 35), "");
        GUI.Label(rect, text, style);
    }
}