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
using MetalamaDependencies = PostSharp.Engineering.BuildTools.Dependencies.Definitions.MetalamaDependencies.V2027_0;

// The .NET 11 SDK, which global.json names as the main SDK of the product and which the build agent installs. The
// version is a literal instead of a member of the product family, because the .NET 11 SDK is a prerelease and
// PostSharp.Engineering names only released feature bands. Keep it equal to the constant of the same name in the
// Metalama repository, and move both to MetalamaDependencies.Family.PreferredVersions.DotNetSdk once the .NET 11
// SDK is released.
const string dotNet11SdkVersion = "11.0.100-rc.1.26425.128";

// The .NET 10 SDK, which stays installed beside the .NET 11 one, because the build tool of this repository targets
// net10.0 and the .NET 11 SDK carries no .NET 10 runtime. The version comes from the product family, so that it
// matches the feature band that the Visual Studio version of the family installs.
var dotNet10SdkVersion = MetalamaDependencies.Family.PreferredVersions.DotNetSdk.V_10_0;

var product = new Product( MetalamaDependencies.MetalamaSamples )
{
    OverriddenBuildAgentRequirements = new ContainerRequirements( ContainerHostKind.Windows )
    {
        Components =
        [
            new DotNetComponent( dotNet11SdkVersion, DotNetComponentKind.Sdk ),
            new DotNetComponent( dotNet10SdkVersion, DotNetComponentKind.Sdk ),
        ]
    },
    GenerateNuGetConfig = true,
    DotNetSdkVersion = new DotNetSdkVersion( dotNet11SdkVersion ) { AllowPrerelease = true },

    
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