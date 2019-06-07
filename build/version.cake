internal class BuildVersion
{
  private BuildVersion()
  {
  }

  public string Branch { get; private set; }
  public string Version { get; private set; }

  public static BuildVersion Instance(ICakeContext context)
  {
    var branch = context.EnvironmentVariable("APPVEYOR_REPO_BRANCH") ?? context.GitBranchCurrent(".").FriendlyName;

    var buildNumber = context.EnvironmentVariable("APPVEYOR_BUILD_NUMBER");

    var version = context.ParseAssemblyInfo("SolutionInfo.cs").AssemblyVersion;

    if (!"master".Equals(branch))
    {
      version = $"{version}-beta";
    }

    if (!"master".Equals(branch) && !string.IsNullOrEmpty(buildNumber))
    {
      version = $"{version}.{buildNumber}";
    }

    return new BuildVersion
    {
      Branch = branch,
      Version = version
    };
  }
}
