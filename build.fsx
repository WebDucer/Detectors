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
let project = "Detector"

// Short summary of the project
let summary = "Implementation of Detector use case"

// Longer description of the project
let description = "Omplementation of Detector use case, if only on of events is relevant."

// List of author names (for NuGet package)
let authors = [ "Eugen [WebDucer] Richter" ]

// Tags for your project (for NuGet package)
let tags = "Detector Event Async"

// File system information
let solutionFile  = "Detector.sln"

// Pattern specifying assemblies to be tested using NUnit
let testAssemblies = "tests/**/bin/Release/*Tests*.dll"

// Base output directory for project
let baseOutput = "Output"

// Output directory for build
let buildOutput = baseOutput @@ "Build"

// Output directory for tests
let testOutput = baseOutput @@ "TestBuild"

// Output directory for artifacts
let artifactOutput = baseOutput @@ "Artifacts"

// NUnit runner path
let nunitToolPath = "packages/fakebuild/NUnit.Runners/tools"

// Read additional information from the release notes document
let release = LoadReleaseNotes "RELEASE_NOTES.md"

// Targets
Description "Cleanup output directories before build"
Target "Cleanup" (fun _ ->
    ReportProgress "Cleanup output folders"

    CleanDirs [baseOutput; buildOutput; artifactOutput]
)

Description "Update assembly info"
Target "UpdateAssembly" (fun _ ->
    ReportProgressStart "Update assembly info"

    BulkReplaceAssemblyInfoVersions "src/" (fun p ->
        {p with
            AssemblyVersion = "0.0.1.0"
            AssemblyFileVersion = "0.0.1.0"
            AssemblyInformationalVersion = "0.0.1.0"
        }
    )

    ReportProgressFinish "Update assembly info"
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
    !! (testOutput + "/**/*Tests.dll")
        |> NUnit (fun p ->
            {p with
                ToolPath = nunitToolPath
                OutputFile = artifactOutput @@ "TestResults.xaml"
            }
          )

    AppVeyor.UploadTestResultsXml AppVeyor.TestResultsType.NUnit artifactOutput
)

"Cleanup"
    ==> "UpdateAssembly"
    ==> "BuildLibrary"
    ==> "BuildTests"
    ==> "RunTests"


RunTargetOrDefault "RunTests"
