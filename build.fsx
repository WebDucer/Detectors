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
let authors = [ "Eugen (WebDucer) Richter" ]

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

// Output directory for artifacts
let artifactOutput = baseOutput @@ "Artifacts"

// Read additional information from the release notes document
let release = LoadReleaseNotes "RELEASE_NOTES.md"

// Targets
Description "Cleanup output directories before build"
Target "Cleanup" (fun _ ->
    CleanDirs [baseOutput; buildOutput; artifactOutput]
)

RunTargetOrDefault "Cleanup"
