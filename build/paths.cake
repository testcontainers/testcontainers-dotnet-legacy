internal class BuildPaths
{
  private BuildPaths()
  {
  }

  public BuildFiles Files { get; private set; }
  public BuildDirectories Directories { get; private set; }

  public static BuildPaths Instance(ICakeContext context, string version)
  {
    var baseDir = (DirectoryPath) context.Directory(".");

    var testResultsDir = baseDir.Combine("test-results");
    var testCoverageDir = baseDir.Combine("test-coverage");

    var artifactsDir = baseDir.Combine("artifacts");
    var artifactsVersionDir = artifactsDir.Combine(version);
    var nugetRoot = artifactsVersionDir.Combine("nuget");

    return new BuildPaths
    {
      Files = new BuildFiles(),
      Directories = new BuildDirectories(
        testResultsDir,
        testCoverageDir,
        nugetRoot
      )
    };
  }
}

internal class BuildFiles
{
  public BuildFiles()
  {
  }
}

internal class BuildDirectories
{
  public DirectoryPath TestResults { get; }
  public DirectoryPath TestCoverage { get; }
  public DirectoryPath NugetRoot { get; }
  public ICollection<DirectoryPath> ToClean { get; }

  public BuildDirectories(
    DirectoryPath testResultsDir,
    DirectoryPath testCoverageDir,
    DirectoryPath nugetRoot)
  {
    TestResults = testResultsDir;
    TestCoverage = testCoverageDir;
    NugetRoot = nugetRoot;
    ToClean = new List<DirectoryPath>()
    {
      testResultsDir,
      testCoverageDir
    };
  }
}
