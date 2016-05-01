// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/fakebuild/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper

// Information about the project are used
//  - for version and project name in generated AssemblyInfo file
//  - by the generated NuGet package

// The name of the project
let project = "WD.Detector"

// Short summary of the project
let summary = "Implementation of Detector use case"

// Longer description of the project
let description = "Omplementation of Detector use case, if only on of events is relevant."

// List of author names (for NuGet package)
let authors = [ "Eugen [WebDucer] Richter" ]

// Tags for your project (for NuGet package)
let tags = "Detector Event Async"

// Base output directory for project
let baseOutput = "Output"

// Output directory for build
let buildOutput = baseOutput @@ "Build"

// Output directory for tests
let testOutput = baseOutput @@ "TestBuild"

// Output directory for temp files
let tempOutput = baseOutput @@ "Temp"

// Output directory for artifacts
let artifactOutput = baseOutput @@ "Artifacts"

// NUnit runner path
let nunitToolPath = "packages/fakebuild/NUnit.Runners/tools"

// Read additional information from the release notes document
let release = LoadReleaseNotes "RELEASE_NOTES.md"

// Version
let buildCounter =
    match buildServer with
    | AppVeyor -> Fake.AppVeyor.AppVeyorEnvironment.BuildNumber
    | Jenkins | TeamCity | Bamboo | Travis | GitLabCI | TeamFoundation -> buildVersion
    | _ -> "0"

// ----------------------------------------------
// START: Version from git tag
// ----------------------------------------------
type ReleaseType = Release | Beta | Alpha | ReleaseCandidate

/// Get the SHA1 for the last available tag or None, if not tags set in repository
let getShaFromLastTag =
    let gitLastTagShaCommand = "rev-list --tags --max-count=1"
    let ok,msg,error = runGitCommand "" gitLastTagShaCommand
    if ok then Some msg.[0] else None

/// Get the tag name for the given SHA1 commit or None, if no tag for this commit
let getTagForCommit sha =
    let gitLastTagCommand = sprintf "describe --tag %s" sha
    let ok,msg,error = runGitCommand "" gitLastTagCommand
    if ok then Some (msg |> Seq.head) else None

/// Get version from the last tag in git or "0.0.0" version as fallback
let getLastTag prefix =
    let fallbackVersion = "0.0.0"
    let tagVersion =
        match getShaFromLastTag with
        | Some s ->
            match getTagForCommit s with
            | Some ss -> ss
            | None _ -> prefix + fallbackVersion
        | None _ -> prefix + fallbackVersion
    let pattern =
        if isNullOrEmpty prefix then "^" else sprintf "(?<=^%s){1}" prefix
    let pattern = sprintf "%s%s" pattern "(\d+\.\d+){1}(\.\d+){0,2}$"
    let versionRegex = System.Text.RegularExpressions.Regex(pattern)
    match versionRegex.Match tagVersion with
    | s when s.Success -> SemVerHelper.parse s.Value
    | _ -> SemVerHelper.parse fallbackVersion

/// Release state
let getReleaseState =
    let currentBranch = Git.Information.getBranchName ""
    match currentBranch with
    | "master" -> Release
    | "develop" -> Beta
    | s when startsWith s "hotfix/" -> ReleaseCandidate
    | s when startsWith s "release/" -> ReleaseCandidate
    | _ -> Alpha

/// Get the version for assembly (+1 on patch level, if not on master)
let getAssemblyVersion prefix =
    let version = getLastTag prefix
    let patch =
        match getReleaseState with
        | Release -> version.Patch
        | _ -> version.Patch + 1
    sprintf "%d.%d.%d.%s" version.Major version.Minor patch buildCounter

/// Get the version for nuget package (+1 on patch level, if not on master)
let getNugetVersion prefix =
    let version = getLastTag prefix
    let releaseState = getReleaseState
    let patch =
        match releaseState with
        | Release -> version.Patch
        | _ -> version.Patch + 1
    match releaseState with
    | Release -> sprintf "%d.%d.%d" version.Major version.Minor patch
    | s -> sprintf "%d.%d.%d-%A%s" version.Major version.Minor patch s buildCounter

/// TRUE: stable realease
let isStableRelease =
    match getReleaseState with
    | Release -> true
    | _ -> false
// ----------------------------------------------
// END: Version from git tag
// ----------------------------------------------

let assemblyVersion = getAssemblyVersion ""
let nugetVersion = getNugetVersion ""

// Targets
Description "Cleanup output directories before build"
Target "Cleanup" (fun _ ->
    CleanDirs [baseOutput; buildOutput; artifactOutput; tempOutput; testOutput]
)

Description "Update assembly info"
Target "UpdateAssembly" (fun _ ->
    BulkReplaceAssemblyInfoVersions "src/" (fun p ->
        {p with
            AssemblyVersion = assemblyVersion
            AssemblyFileVersion = assemblyVersion
            AssemblyInformationalVersion = assemblyVersion
        }
    )
)

Description "Build library"
Target "BuildLibrary" (fun _ ->
    !! "src/**/*csproj"
        |> MSBuildRelease buildOutput "Rebuild"
        |> Log "Build Output: "
)

Description "Build tests"
Target "BuildTests" (fun _ ->
    !! "tests/**/*.csproj"
        |> MSBuildRelease testOutput "Rebuild"
        |> Log "Test Build Output: "
)

Description "Run tests with NUnit 2"
Target "RunTests" (fun _ ->
    let resultFile = artifactOutput @@ "TestResults.xaml"
    !! (testOutput + "/**/*Tests.dll")
        |> NUnit (fun p ->
            {p with
                ToolPath = nunitToolPath
                OutputFile = resultFile
            }
          )
)

Description "Publish test results"
Target "PublishTestResults" (fun _ ->
    let resultFile = artifactOutput @@ "TestResults.xaml"
    AppVeyor.UploadTestResultsFile AppVeyor.TestResultsType.NUnit resultFile
)

Description "Create nuget package of the library"
Target "CreatePackage" (fun _ ->
    NuGet (fun p ->
        {p with
            Authors = authors
            Project = project
            Version = nugetVersion
            OutputPath = artifactOutput
            Summary = summary
            Description = description
            WorkingDir = "./"
            Tags = tags
            Publish = false
            Files = 
            [
                (buildOutput @@ "WD.Detector.dll", Some "lib/net45", None)
            ]
        }
    ) "Detectors.nupkg"
)

Description "Publish artifacts"
Target "PublishArtifacts" (fun _ ->
    AppVeyor.PushArtifacts (!! (artifactOutput + "/**/*.nupkg"))
)

Description "Finish Task"
Target "All" (fun _ ->
    trace "Build finished"
)

"Cleanup"
    ==> "UpdateAssembly"
    ==> "BuildLibrary"
    ==> "BuildTests"
    ==> "RunTests"
    ==> "CreatePackage"
    =?> ("PublishTestResults", buildServer = BuildServer.AppVeyor)
    =?> ("PublishArtifacts", buildServer = BuildServer.AppVeyor)
    ==> "All"

RunTargetOrDefault "All"
