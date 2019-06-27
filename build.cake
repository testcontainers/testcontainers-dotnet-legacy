#addin nuget:?package=Cake.Git&version=0.19.0

#load "./build/parameters.cake"

readonly var param = BuildParameters.Instance(Context, "TestContainers");

Setup(context =>
{
  var toClean = param.Paths.Directories.ToClean;

  foreach (var project in param.Projects.All)
  {
    toClean.Add(project.Path.GetDirectory().Combine("obj"));
    toClean.Add(project.Path.GetDirectory().Combine("bin"));
    toClean.Add(project.Path.GetDirectory().Combine("Release"));
    toClean.Add(project.Path.GetDirectory().Combine("Debug"));
  }

  Information("Building version {0} of .NET Testcontainers ({1})", param.Version, param.Branch);
});

Teardown(context =>
{
});

Task("Clean")
  .Does(() =>
{
  var deleteDirectorySettings = new DeleteDirectorySettings();
  deleteDirectorySettings.Recursive = true;
  deleteDirectorySettings.Force = true;

  foreach (var directory in param.Paths.Directories.ToClean)
  {
    if (DirectoryExists(directory))
    {
      DeleteDirectory(directory, deleteDirectorySettings);
    }
  }
});

Task("Restore-NuGet-Packages")
  .Does(() =>
{
  DotNetCoreRestore(param.Solution, new DotNetCoreRestoreSettings
  {
    Verbosity = param.Verbosity
  });
});

Task("Build-Informations")
  .Does(() =>
{
  foreach (var project in param.Projects.All)
  {
    Console.WriteLine($"{project.Name} [{project.Path.GetDirectory()}]");
  }
});

Task("Build")
  .Does(() =>
{
  DotNetCoreBuild(param.Solution, new DotNetCoreBuildSettings
  {
    Configuration = param.Configuration,
    Verbosity = param.Verbosity,
    NoRestore = true
  });
});

Task("Test")
  .Does(() =>
{
  foreach(var testProject in param.Projects.OnlyTests)
  {
    DotNetCoreTest(testProject.Path.FullPath, new DotNetCoreTestSettings
    {
      Configuration = param.Configuration,
      Verbosity = param.Verbosity,
      NoRestore = true,
      NoBuild = true,
      Logger = "trx",
      ResultsDirectory = param.Paths.Directories.TestResults,
      ArgumentCustomization = args => args
        .Append("/p:CollectCoverage=true")
        .Append("/p:CoverletOutputFormat=opencover")
        .Append($"/p:CoverletOutput=\"{MakeAbsolute(param.Paths.Directories.TestCoverage)}/\"")
    });
  }
});

Task("Create-NuGet-Packages")
  .Does(() =>
{
  DotNetCorePack(param.Projects.Testcontainers.Path.FullPath, new DotNetCorePackSettings
  {
    Configuration = param.Configuration,
    Verbosity = param.Verbosity,
    NoRestore = true,
    NoBuild = true,
    IncludeSymbols = true,
    OutputDirectory = param.Paths.Directories.NugetRoot,
    ArgumentCustomization = args => args
      .Append($"/p:Version={param.Version}")
      .Append("/p:SymbolPackageFormat=snupkg")
  });
});

Task("Publish-NuGet-Packages")
  .WithCriteria(() => param.ShouldPublish)
  .Does(() =>
{
  foreach(var package in GetFiles($"{param.Paths.Directories.NugetRoot}/*.(nupkg|snupkgs)"))
  {
    NuGetPush(package, new NuGetPushSettings
    {
      Source = param.NuGetCredentials.Source,
      ApiKey = param.NuGetCredentials.ApiKey
    });
  }
});

Task("Default")
  .IsDependentOn("Clean")
  .IsDependentOn("Restore-NuGet-Packages")
  .IsDependentOn("Build")
  .IsDependentOn("Test");

RunTarget(param.Target);
