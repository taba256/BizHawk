static class VersionInfo
{
	public const string MAINVERSION = "1.7.3";
	public static string RELEASEDATE = "July 22, 2014";
	public static bool DeveloperBuild = false;

	public static string GetEmuVersion()
	{
		return DeveloperBuild ? "SVN " + SubWCRev.SVN_REV : ("Version " + MAINVERSION);
	}
}
