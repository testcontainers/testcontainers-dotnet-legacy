#load "./paths.cake"
#load "./projects.cake"

internal class BuildParameters
{
  private BuildParameters()
  {
  }

  public string Solution { get; private set; }
  public string Target { get; private set; }
  public string Configuration { get; private set; }
  public string Version { get; private set; }
  public string Branch { get; private set; }
  public DotNetCoreVerbosity Verbosity { get; private set; }
  public BuildProjects Projects { get; private set; }
  public BuildPaths Paths { get; private set; }

  public static BuildParameters Instance(ICakeContext context, string solution)
  {
    var version = "0.0.2";

    var branch = "develop";

    return new BuildParameters
    {
      Solution = context.MakeAbsolute(new DirectoryPath($"{solution}.sln")).FullPath,
      Target = context.Argument("target", "Default"),
      Configuration = context.Argument("configuration", "Debug"),
      Version = version,
      Branch = branch,
      Verbosity = DotNetCoreVerbosity.Quiet,
      Projects = BuildProjects.Instance(context, solution),
      Paths = BuildPaths.Instance(context, version)
    };
  }
}
