// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using PostSharp.Engineering.BuildTools;
using PostSharp.Engineering.BuildTools.Build;
using PostSharp.Engineering.BuildTools.Build.Model;
using PostSharp.Engineering.BuildTools.Build.Solutions;
using PostSharp.Engineering.BuildTools.Dependencies.Definitions;
using PostSharp.Engineering.BuildTools.Docker;
using System.IO;
using System.IO.Compression;
using MetalamaDependencies = PostSharp.Engineering.BuildTools.Dependencies.Definitions.MetalamaDependencies.V2026_1;

var preferredVersions = MetalamaDependencies.Family.PreferredVersions;

var dotNetSdkVersion = preferredVersions.DotNetSdk.V_10_0;

var product = new Product( MetalamaDependencies.MetalamaSamples )
{
    OverriddenBuildAgentRequirements = new ContainerRequirements( ContainerHostKind.Windows )
    {
        Components =
        [
            new DotNetComponent( dotNetSdkVersion, DotNetComponentKind.Sdk ),
        ]
    },
    GenerateNuGetConfig = true,
    DotNetSdkVersion = new DotNetSdkVersion( preferredVersions.DotNetSdk.V_10_0 ),

    
    Solutions =
    [
        new DotNetSolution( "Metalama.Samples.sln" ) { 
        CanFormatCode = true, 
        // We must build all projects because we produce HTML formatted files and include them in the artifacts.
        PackRequiresExplicitBuild = true }
    ],
    TestOnBuild = true,
    MainVersionDependency = MetalamaDependencies.Metalama,
    PublicArtifacts = Pattern.Create(
        "Metalama.Documentation.QuickStart.$(PackageVersion).nupkg" ),
};

product.TestCompleted += OnTestCompleted;

return new EngineeringApp( product ).Run( args );

void OnTestCompleted( BuildCompletedEventArgs args )
{
    var sourceDirectory = Path.Combine( args.Context.RepoDirectory, "src" );

    var matcher = new Matcher();
    matcher.AddInclude( "**/*.html" );
    var matches = matcher.Execute( new DirectoryInfoWrapper( new DirectoryInfo( sourceDirectory ) ) );

    // Store each matched file to a zip file in the destination directory
    var targetZipFile = Path.Combine( args.PrivateArtifactsDirectory, "html-examples.zip" );

    if ( File.Exists( targetZipFile ) )
    {
        File.Delete( targetZipFile );
    }

    using var archive = ZipFile.Open( targetZipFile, ZipArchiveMode.Create );

    foreach ( var match in matches.Files )
    {
        var sourceFile = Path.Combine( sourceDirectory, match.Path );
        archive.CreateEntryFromFile( sourceFile, match.Path );
    }
}