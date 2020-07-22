internal class BuildVersion
{
  private BuildVersion()
  {
  }

  public string Branch { get; private set; }
  public string Version { get; private set; }

  public static BuildVersion Instance(ICakeContext context)
  {
    var branch = new[]
    {
      context.EnvironmentVariable("APPVEYOR_PULL_REQUEST_HEAD_REPO_BRANCH"),
      context.EnvironmentVariable("APPVEYOR_REPO_BRANCH"),
      context.GitBranchCurrent(".").FriendlyName
    }.First(name => !string.IsNullOrEmpty(name));

    var buildNumber = context.EnvironmentVariable("APPVEYOR_BUILD_NUMBER");

    var version = context.XmlPeek("Shared.msbuild", "/Project/PropertyGroup[1]/Version/text()");

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
