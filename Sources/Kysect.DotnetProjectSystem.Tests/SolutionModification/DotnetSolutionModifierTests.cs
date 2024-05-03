using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Tests.Asserts;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Language.Xml;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.SolutionModification;

public class DotnetSolutionModifierTests
{
    private readonly MockFileSystem _fileSystem;
    private readonly XmlDocumentSyntaxFormatter _syntaxFormatter;
    private readonly DotnetSolutionModifierFactory _solutionModifierFactory;
    private readonly FileSystemAsserts _fileSystemAsserts;
    private readonly string _currentPath;
    private readonly SolutionFileStructureBuilderFactory _solutionFileStructureBuilderFactory;

    public DotnetSolutionModifierTests()
    {
        _fileSystem = new MockFileSystem();
        _syntaxFormatter = new XmlDocumentSyntaxFormatter();
        var solutionFileContentParser = new SolutionFileContentParser();
        _solutionModifierFactory = new DotnetSolutionModifierFactory(_fileSystem, solutionFileContentParser, _syntaxFormatter);
        _currentPath = _fileSystem.Path.GetFullPath(".");
        _fileSystemAsserts = new FileSystemAsserts(_fileSystem);
        _solutionFileStructureBuilderFactory = new SolutionFileStructureBuilderFactory(_fileSystem, _syntaxFormatter);
    }

    [Fact]
    public void Save_WithoutChanges_FinishWithoutErrors()
    {
        string projectContent = """
                                <Project Sdk="Microsoft.NET.Sdk">
                                  <PropertyGroup>
                                    <TargetFramework>net8.0</TargetFramework>
                                    <ImplicitUsings>enable</ImplicitUsings>
                                    <Nullable>enable</Nullable>
                                  </PropertyGroup>

                                  <ItemGroup>
                                    <PackageReference Include="FluentAssertions" />
                                    <PackageReference Include="Microsoft.NET.Test.Sdk" />
                                  </ItemGroup>
                                </Project>
                                """;

        _solutionFileStructureBuilderFactory.Create("Solution")
            .AddProject(
                new ProjectFileStructureBuilder("SampleProject")
                    .SetContent(projectContent))
            .Save(_currentPath);

        var solutionModifier = _solutionModifierFactory.Create("Solution.sln");
        solutionModifier.Save();
    }

    [Fact]
    public void Save_AfterAddingValueToDirectoryBuildProps_FileSaved()
    {
        var expectedContent = """
                              <Project>
                                <PropertyGroup>
                                  <Key>value</Key>
                                </PropertyGroup>
                              </Project>
                              """;

        _solutionFileStructureBuilderFactory.Create("Solution")
            .Save(_currentPath);

        DotnetSolutionModifier solutionModifier = _solutionModifierFactory.Create("Solution.sln");
        solutionModifier
            .GetOrCreateDirectoryBuildPropsModifier()
            .File
            .Properties
            .AddProperty("Key", "value");
        solutionModifier.Save();

        _fileSystemAsserts
            .File(SolutionItemNameConstants.DirectoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(expectedContent);
    }

    [Fact]
    public void Save_AfterModifyDirectoryBuildTargets_FileSaved()
    {
        var expectedContent = """
                              <Project>
                                <NewNode></NewNode>
                              </Project>
                              """;

        _solutionFileStructureBuilderFactory.Create("Solution")
            .AddDirectoryBuildTargets(DirectoryBuildTargetFile.CreateEmpty())
            .Save(_currentPath);

        DotnetSolutionModifier solutionModifier = _solutionModifierFactory.Create("Solution.sln");
        solutionModifier
            .GetOrCreateDirectoryBuildTargetFile()
            .UpdateDocument(d => d.ReplaceNode(
                d.Root.AsSyntaxElement.AsNode,
                d.Root.AsSyntaxElement.AddChild(ExtendedSyntaxFactory.XmlElement("NewNode")).AsNode));
        solutionModifier.Save();

        _fileSystemAsserts
            .File(SolutionItemNameConstants.DirectoryBuildTargets)
            .ShouldExists()
            .ShouldHaveContent(expectedContent);
    }
}