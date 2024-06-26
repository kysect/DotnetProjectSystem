﻿using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Xml;

namespace Kysect.DotnetProjectSystem.Tests.Projects;

public class DirectoryBuildPropsFileTests
{
    private readonly XmlDocumentSyntaxFormatter _formatter;

    public DirectoryBuildPropsFileTests()
    {
        _formatter = new XmlDocumentSyntaxFormatter();
    }

    [Fact]
    public void ArtifactsOutputEnabled_ForEmptyFile_ReturnFalse()
    {
        const string input = """
                             <Project>
                             </Project>
                             """;

        var directoryPackagesPropsFile = new DirectoryBuildPropsFile(input);

        bool actual = directoryPackagesPropsFile.ArtifactsOutputEnabled();

        actual.Should().Be(false);
    }

    [Fact]
    public void ArtifactsOutputEnabled_ForEnabled_ReturnTrue()
    {
        const string input = """
                             <Project>
                               <PropertyGroup>
                                 <UseArtifactsOutput>true</UseArtifactsOutput>
                               </PropertyGroup>
                             </Project>
                             """;

        var directoryPackagesPropsFile = new DirectoryBuildPropsFile(input);

        bool actual = directoryPackagesPropsFile.ArtifactsOutputEnabled();

        actual.Should().Be(true);
    }

    [Fact]
    public void ArtifactsOutputEnabled_ForDisabled_ReturnTrue()
    {
        const string input = """
                             <Project>
                               <PropertyGroup>
                                 <UseArtifactsOutput>false</UseArtifactsOutput>
                               </PropertyGroup>
                             </Project>
                             """;

        var directoryPackagesPropsFile = new DirectoryBuildPropsFile(input);

        bool actual = directoryPackagesPropsFile.ArtifactsOutputEnabled();

        actual.Should().Be(false);
    }

    [Fact]
    public void SetArtifactsOutput_ForEmptyFile_CreateExpectedString()
    {
        const string expected = """
                             <Project>
                               <PropertyGroup>
                                 <UseArtifactsOutput>true</UseArtifactsOutput>
                               </PropertyGroup>
                             </Project>
                             """;

        var directoryPackagesPropsFile = DirectoryBuildPropsFile.CreateEmpty();

        directoryPackagesPropsFile.SetArtifactsOutput(true);

        directoryPackagesPropsFile.File.ToXmlString(_formatter).Should().Be(expected);
    }
}