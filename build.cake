#tool "nuget:?package=GitVersion.CommandLine"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var project = "TestContainers";

Task("Version")
    .Does(() => {
        GitVersion(new GitVersionSettings
        {
            UpdateAssemblyInfo = true,
            OutputType = GitVersionOutput.BuildServer
        });
    });

Task("Restore")
    .Does(() =>
{
    DotNetCoreRestore(project);
});

Task("Build")
    .Does(() => 
{
    DotNetCoreBuild(project, new DotNetCoreBuildSettings {
        Configuration = configuration
    });
});

Task("Default")
    //.IsDependentOn("Clean")
    //.IsDependentOn("Version")
    .IsDependentOn("Restore")
    .IsDependentOn("Build");


RunTarget(target);