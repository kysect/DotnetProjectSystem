using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Traversing;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.Traversing;

public class DotnetSolutionSourceFileFinderTests
{
    private readonly DotnetSolutionParser _solutionStructureParser;
    private readonly DotnetSolutionSourceFileFinder _sourceFileFinder;
    private readonly MockFileSystem _fileSystem;
    private readonly XmlDocumentSyntaxFormatter _syntaxFormatter;

    public DotnetSolutionSourceFileFinderTests()
    {
        ILogger logger = DefaultLoggerConfiguration.CreateConsoleLogger();

        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        _solutionStructureParser = new DotnetSolutionParser(_fileSystem, logger);
        _sourceFileFinder = new DotnetSolutionSourceFileFinder(_fileSystem, logger);
        _syntaxFormatter = new XmlDocumentSyntaxFormatter();
    }

    [Fact]
    public void FindSourceFiles_ProjectWithDefaultItems_ReturnExpectedResult()
    {
        string projectContent = """
                                <Project Sdk="Microsoft.NET.Sdk">
                                  <PropertyGroup>
                                    <TargetFramework>net8.0</TargetFramework>
                                    <ImplicitUsings>enable</ImplicitUsings>
                                    <Nullable>enable</Nullable>
                                  </PropertyGroup>
                                </Project>
                                """;

        string currentPath = _fileSystem.Path.GetFullPath(".");
        string fullPathToFirstFile = _fileSystem.Path.Combine(currentPath, "SampleProject", "File1.cs");
        string pathToSecondFile = _fileSystem.Path.Combine(currentPath, "SampleProject", "InnerDirectory", "File2.cs");

        var expectedProjectPaths = new DotnetProjectPaths(
            _fileSystem.Path.Combine(currentPath, "SampleProject", "SampleProject.csproj"),
            new[] { fullPathToFirstFile, pathToSecondFile });

        var expected = new DotnetSolutionPaths(
            _fileSystem.Path.Combine(currentPath, "Solution.sln"),
            new[] { expectedProjectPaths });

        var solutionBuilder = new SolutionFileStructureBuilder("Solution")
            .AddProject(
                new ProjectFileStructureBuilder("SampleProject", projectContent)
                    .AddEmptyFile("File1.cs")
                    .AddEmptyFile("InnerDirectory", "File2.cs"));

        solutionBuilder.Save(_fileSystem, currentPath, _syntaxFormatter);
        DotnetSolutionDescriptor dotnetSolutionDescriptor = _solutionStructureParser.Parse("Solution.sln");
        DotnetSolutionPaths dotnetSolutionPaths = _sourceFileFinder.FindSourceFiles(dotnetSolutionDescriptor);

        dotnetSolutionPaths.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void FindSourceFiles_ProjectWithDefaultItemsAndBinObjDirectories_ReturnExpectedResult()
    {
        string projectContent = """
                                <Project Sdk="Microsoft.NET.Sdk">
                                  <PropertyGroup>
                                    <TargetFramework>net8.0</TargetFramework>
                                    <ImplicitUsings>enable</ImplicitUsings>
                                    <Nullable>enable</Nullable>
                                  </PropertyGroup>
                                </Project>
                                """;

        string currentPath = _fileSystem.Path.GetFullPath(".");
        string fullPathToFirstFile = _fileSystem.Path.Combine(currentPath, "SampleProject", "File1.cs");
        string pathToSecondFile = _fileSystem.Path.Combine(currentPath, "SampleProject", "InnerDirectory", "File2.cs");

        var expectedProjectPaths = new DotnetProjectPaths(
            _fileSystem.Path.Combine(currentPath, "SampleProject", "SampleProject.csproj"),
            new[] { fullPathToFirstFile, pathToSecondFile });

        var expected = new DotnetSolutionPaths(
            _fileSystem.Path.Combine(currentPath, "Solution.sln"),
            new[] { expectedProjectPaths });

        var solutionBuilder = new SolutionFileStructureBuilder("Solution")
            .AddProject(
                new ProjectFileStructureBuilder("SampleProject", projectContent)
                    .AddEmptyFile("File1.cs")
                    .AddEmptyFile("InnerDirectory", "File2.cs")
                    .AddEmptyFile("bin", "Bin.cs")
                    .AddEmptyFile("obj", "Obj.cs"));

        solutionBuilder.Save(_fileSystem, currentPath, _syntaxFormatter);
        DotnetSolutionDescriptor dotnetSolutionDescriptor = _solutionStructureParser.Parse("Solution.sln");
        DotnetSolutionPaths dotnetSolutionPaths = _sourceFileFinder.FindSourceFiles(dotnetSolutionDescriptor);

        dotnetSolutionPaths.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void FindSourceFiles_LegacyProject_ReturnExpectedResult()
    {
        string projectContent = """
                                <Project ToolsVersion="15.0">
                                  <PropertyGroup>
                                    <TargetFramework>net8.0</TargetFramework>
                                    <ImplicitUsings>enable</ImplicitUsings>
                                    <Nullable>enable</Nullable>
                                  </PropertyGroup>
                                  <ItemGroup>
                                    <Compile Include="File1.cs" />
                                  </ItemGroup>
                                </Project>
                                """;

        string currentPath = _fileSystem.Path.GetFullPath(".");
        string fullPathToFirstFile = _fileSystem.Path.Combine(currentPath, "SampleProject", "File1.cs");
        string pathToSecondFile = _fileSystem.Path.Combine(currentPath, "SampleProject", "File2.cs");

        var expectedProjectPaths = new DotnetProjectPaths(
            _fileSystem.Path.Combine(currentPath, "SampleProject", "SampleProject.csproj"),
            new[] { fullPathToFirstFile });

        var expected = new DotnetSolutionPaths(
            _fileSystem.Path.Combine(currentPath, "Solution.sln"),
            new[] { expectedProjectPaths });

        var solutionBuilder = new SolutionFileStructureBuilder("Solution")
            .AddProject(
                new ProjectFileStructureBuilder("SampleProject")
                    .SetContent(
                        DotnetProjectFile
                            .Create(projectContent))
                    .AddEmptyFile("File1.cs")
                    .AddEmptyFile("File2.cs"));

        solutionBuilder.Save(_fileSystem, currentPath, _syntaxFormatter);
        DotnetSolutionDescriptor dotnetSolutionDescriptor = _solutionStructureParser.Parse("Solution.sln");
        DotnetSolutionPaths dotnetSolutionPaths = _sourceFileFinder.FindSourceFiles(dotnetSolutionDescriptor);

        dotnetSolutionPaths.Should().BeEquivalentTo(expected);
    }
}