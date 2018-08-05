using Assets.Scripts.Game.UI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpeakingPNJ))]
public class EditorHelpText : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
	}
}
