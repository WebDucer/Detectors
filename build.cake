// Tools
#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=NUnit.ConsoleRunner"

// Add Ins
#addin "Cake.FileHelpers"

// Target
var target = Argument("target", "Default");

// Paths
var solutionPath = File("Detector.sln");
var baseOutput = Directory("Output");
var buildOutput = baseOutput + Directory("Build");
var buildOutputNet45 = buildOutput + Directory("net45");
var buildOutputNetStandard = buildOutput + Directory("netstandard");
var buildOutputPCL = buildOutput + Directory("pcl");
var testOutput = baseOutput + Directory("TestBuild");
var artifactOutput = baseOutput + Directory("Artifacts");

// Meta informations
var projectName = "WD.Detector";
var summary = "Implementation of Detector use case";
var description = "Implementation of Detector use case, if only one of the events is relevant.";
var authors = new [] { "Eugen [WebDucer] Richter" };
var tags = new [] { "Detector", "Event", "Async" };
var copyright = "MIT - WebDucer (c) " + DateTime.Now.Year;
var projectUri = new Uri("https://bitbucket.org/webducertutorials/detectors");
var licenceUri = new Uri("https://bitbucket.org/webducertutorials/detectors/src/master/LICENSE.txt");
var releaseNotes = FileReadLines("RELEASE_NOTES.md");

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
        var testResultFile = artifactOutput + File("TestResults.xml");
        var testFiles = GetFiles(testOutput.Path.FullPath + "/*Tests.dll");
        var testSetiings = new NUnit3Settings {
            Results = testResultFile
        };
        NUnit3(testFiles, testSetiings);

        if(BuildSystem.IsRunningOnAppVeyor) {
            BuildSystem.AppVeyor.UploadTestResults(testResultFile, AppVeyorTestResultsType.NUnit3);
        }
    });

/*
 * Patch assembly version
 */
Task("PatchAssemblyVersion")
    .IsDependentOn("RestoreDependencies")
    .Does(() => {
        var versionSettings = new GitVersionSettings {
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
        // Net 45
        var toBuildFolder = MakeAbsolute(buildOutputNet45);
        var buildProjects = GetFiles("./src/**/Detector.csproj");
        foreach(var project in buildProjects){
            buildProject(project, toBuildFolder);
        }

        // NetStandard
        toBuildFolder = MakeAbsolute(buildOutputNetStandard);
        buildProjects = GetFiles("./src/**/Detector.NetStandard.csproj");
        foreach(var project in buildProjects){
            buildProject(project, toBuildFolder);
        }

        // PCL
        toBuildFolder = MakeAbsolute(buildOutputPCL);
        buildProjects = GetFiles("./src/**/Detector.PCL.csproj");
        foreach(var project in buildProjects){
            buildProject(project, toBuildFolder);
        }
    });

/*
 * Create NuGet package
 */
Task("CreatePackage")
    .IsDependentOn("RunTests")
    .IsDependentOn("BuildLibrary")
    .Does(() => {
        var libFolderNet45 = Directory("lib") + Directory("net45");
        var libFolderNetStandard = Directory("lib") + Directory("netstandard1.1");
        var libFolderPcl = Directory("lib") + Directory("portable-net45+wp8+win8");
        var libFolderAndroid = Directory("lib") + Directory("MonoAndroid");
        var libFolderIos = Directory("lib") + Directory("xamarinios");

        var nugetSettings = new NuGetPackSettings {
            Id = projectName,
            Title = projectName,
            Summary = summary,
            Description = description,
            Authors = authors,
            Owners = authors,
            Version = version.NuGetVersion,
            Tags = tags,
            OutputDirectory = artifactOutput,
            Copyright = copyright,
            BasePath = "./",
            ProjectUrl = projectUri,
            LicenseUrl = licenceUri,
            ReleaseNotes = releaseNotes,
            Files = new [] {
                new NuSpecContent {
                    Source = buildOutputNet45 + File("WD.Detector.dll"),
                    Target = libFolderNet45
                },
                new NuSpecContent {
                    Source = buildOutputNetStandard + File("WD.Detector.dll"),
                    Target = libFolderNetStandard
                },
                new NuSpecContent {
                    Source = buildOutputPCL + File("WD.Detector.dll"),
                    Target = libFolderPcl
                },
                new NuSpecContent {
                    Source = buildOutputPCL + File("WD.Detector.dll"),
                    Target = libFolderAndroid
                },
                new NuSpecContent {
                    Source = buildOutputPCL + File("WD.Detector.dll"),
                    Target = libFolderIos
                }
            }
        };

        NuGetPack(nugetSettings);
    });

Task("Default")
    .IsDependentOn("CreatePackage");

/*
 * Run build
 */
RunTarget(target);