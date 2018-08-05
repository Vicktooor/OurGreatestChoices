using Assets.Scripts.Game.UI;
using Assets.Scripts.Game.UI.Ftue;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FtueManager))]
public class FtueConstructor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		GUILayout.Space(25);
		if (GUILayout.Button("ADD ONE")) Add();
		if (GUILayout.Button("REMOVE ONE")) Remove();
	}

	public void Add()
	{
		int targetIndex = FtueManager.instance.modificationStepTarget;
		FtueComponent[] newSteps = new FtueComponent[FtueManager.instance.steps.Count + 1];
		bool added = false;
		for (int i = 0; i <= FtueManager.instance.steps.Count; i++)
		{
			if (added && i >= FtueManager.instance.steps.Count) break;
			if (i == targetIndex)
			{
				added = true;
				newSteps[i] = FtueManager.instance.stepToAdd.Copy();
			}
			else
			{
				if (added) newSteps[i] = FtueManager.instance.steps[i - 1];
				else newSteps[i] = FtueManager.instance.steps[i];
			}
		}

		if (added)
		{
			List<FtueComponent> newList = new List<FtueComponent>();
			for (int i = 0; i < newSteps.Length; i++) newList.Add(newSteps[i]);
			FtueManager.instance.steps = newList;
		}
	}

	public void Remove()
	{
		int targetIndex = FtueManager.instance.modificationStepTarget - 1;
		FtueComponent[] newSteps = new FtueComponent[FtueManager.instance.steps.Count - 1];
		int index = 0;
		bool removed = false;
		for (int i = 0; i < FtueManager.instance.steps.Count; i++)
		{
			if (i != targetIndex)
			{
				newSteps[index] = FtueManager.instance.steps[i];
				index++;
			}
			else removed = true;
		}

		if (removed)
		{
			List<FtueComponent> newList = new List<FtueComponent>();
			for (int i = 0; i < newSteps.Length; i++) newList.Add(newSteps[i]);
			FtueManager.instance.steps = newList;
		}
	}
}