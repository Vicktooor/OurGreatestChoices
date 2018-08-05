using Boo.Lang;
using System;
using System.IO;
using UnityEngine;

public class FileProcess
{
	public static string[] GetFileNames(string targetDirectory)
	{
		List<string> filesPath = new List<string>();
		ProcessDirectory(targetDirectory, filesPath);
		return filesPath.ToArray();
	}

	private static void ProcessDirectory(string targetDirectory, List<string> pathArray)
	{
		string[] fileEntries = Directory.GetFiles(targetDirectory);
		foreach (string fileName in fileEntries)
			ProcessFile(fileName, pathArray);

		string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
		foreach (string subdirectory in subdirectoryEntries)
			ProcessDirectory(subdirectory, pathArray);
	}

	private static void ProcessFile(string path, List<string> pathArray)
	{
		string[] cutPath = path.Split(new char[2] { '/', '.' });
		if (cutPath[cutPath.Length - 1] == "meta") return;
		else if (cutPath[cutPath.Length - 1] == "prefab" || cutPath[cutPath.Length - 1] == "asset")
			pathArray.Add(cutPath[cutPath.Length - 2]);
	}
}
