
public static class PostBuild
{
	/*[PostProcessBuild]
	static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
	{
		string path = Path.GetDirectoryName(pathToBuiltProject) + "/" + Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data";
		FileUtil.CopyFileOrDirectory("Assets/Save", path + "/somefolder");
	}*/
}
