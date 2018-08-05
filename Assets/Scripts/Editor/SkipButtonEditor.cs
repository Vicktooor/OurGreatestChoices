using UnityEditor;

[CustomEditor(typeof(SkipButton))]
public class SkipButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SkipButton targetMenuButton = (SkipButton)target;
        // Show default inspector property editor
        DrawDefaultInspector();
    }
}