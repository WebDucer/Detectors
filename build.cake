// Tools
#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=NUnit.ConsoleRunner"

// Target
var target = Argument("target", "Default");

// Paths
var solutionPath = File("Detector.sln");
var baseOutput = Directory("Output");
var buildOutput = baseOutput + Directory("Build");
var testOutput = baseOutput + Directory("TestBuild");
var artifactOutput = baseOutput + Directory("Artifacts");

// Meta informations
var projectName = "WD.Detector";
var summary = "Implementation of Detector use case";
var description = "Implementation of Detector use case, if only one of the events is relevant.";
var authors = new [] { "Eugen [WebDucer] Richter" };
var tags = "Detector Event Async";

// Version
var version = GitVersion();

// Cross platform build
var buildProject = new Action<FilePath, DirectoryPath>((FilePath projectFile, DirectoryPath outputFolder) => {
    if(IsRunningOnUnix()){
        var buildSettings = new XBuildSettings{
            Configuration = "Release"
        }.WithProperty("OutDir", outputFolder.FullPath + "/");
        XBuild(projectFile, buildSettings);
    } else {
        var buildSettings = new MSBuildSettings{
            Configuration = "Release",
            NodeReuse = true,
            MaxCpuCount = 0,
        }.WithProperty("OutDir", outputFolder.FullPath);
        MSBuild(projectFile, buildSettings);
    }
});

/*
 * Remove all old files from build and test directories
 */
Task("Cleanup")
    .Does(() => {
        var directories = new DirectoryPath[] {buildOutput, testOutput, artifactOutput};
        CleanDirectories(directories);
    });

/*
 * Restore all nuget packages of the solution
 */
Task("RestoreDependencies")
    .IsDependentOn("Cleanup")
    .Does(() => {
        NuGetRestore(solutionPath);
    });

/*
 * Build test projects
 */
Task("BuildTests")
    .IsDependentOn("RestoreDependencies")
    .Does(() => {
        var toBuildFolder = MakeAbsolute(testOutput);
        var testProjects = GetFiles("./tests/**/Detector.Tests.csproj");

        foreach(var project in testProjects){
            buildProject(project, toBuildFolder);
        }
    });

/*
 * Run NUnit3 tests
 */
Task("RunTests")
    .IsDependentOn("BuildTests")
    .Does(() => {
        var testFiles = GetFiles(testOutput.Path.FullPath + "/*Tests.dll");
        var testSetiings = new NUnit3Settings {
            Results = artifactOutput + File("TestResults.xml")
        };
        NUnit3(testFiles, testSetiings);
    });

/*
 * Patch assembly version
 */
Task("PatchAssemblyVersion")
    .IsDependentOn("RestoreDependencies")
    .Does(() => {
        var versionSettings = new GitVersinSettings {
            UpdateAssemblyInfo = true
        };
        GitVersion(versionSettings);
    });

/*
 * Build the library
 */
Task("BuildLibrary")
    .IsDependentOn("RestoreDependencies")
    .IsDependentOn("PatchAssemblyVersion")
    .Does(() => {

    });

Task("Default")
    .IsDependentOn("RunTests");

/*
 * Run build
 */
RunTarget(target);