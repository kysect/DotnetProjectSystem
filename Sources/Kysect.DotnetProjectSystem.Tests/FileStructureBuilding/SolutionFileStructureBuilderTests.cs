using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Tests.Asserts;
using Kysect.DotnetProjectSystem.Xml;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.FileStructureBuilding;

public class SolutionFileStructureBuilderTests
{
    private readonly string _rootPath;
    private readonly MockFileSystem _fileSystem;
    private readonly FileSystemAsserts _asserts;
    private readonly XmlDocumentSyntaxFormatter _syntaxFormatter;

    public SolutionFileStructureBuilderTests()
    {
        _fileSystem = new MockFileSystem();
        _rootPath = _fileSystem.Path.GetFullPath(".");
        _asserts = new FileSystemAsserts(_fileSystem);
        _syntaxFormatter = new XmlDocumentSyntaxFormatter();
    }

    [Fact]
    public void Save_ForEmptySolution_CreateSolutionFile()
    {
        string solutionName = "MySolution";

        new SolutionFileStructureBuilder(solutionName)
            .Save(_fileSystem, _rootPath, _syntaxFormatter);

        _asserts
            .File(_rootPath, $"{solutionName}.sln")
            .ShouldExists();
    }

    [Fact]
    public void Save_AfterAddingFile_FileShouldExists()
    {
        string solutionName = "MySolution";
        string directoryBuildProps = "Directory.Build.props";
        string content = "<Project></Project>";

        new SolutionFileStructureBuilder(solutionName)
            .AddFile([directoryBuildProps], content)
            .Save(_fileSystem, _rootPath, _syntaxFormatter);

        _asserts
            .File(_rootPath, $"{solutionName}.sln")
            .ShouldExists();

        _asserts
            .File(_rootPath, directoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(content);
    }

    [Fact]
    public void Save_AfterAddingProject_ProjectFileShouldExists()
    {
        string solutionName = "MySolution";
        string projectName = "Project";
        string content = "<Project></Project>";

        new SolutionFileStructureBuilder(solutionName)
            .AddProject(
                new ProjectFileStructureBuilder(projectName)
                    .SetContent(content))
            .Save(_fileSystem, _rootPath, _syntaxFormatter);

        _asserts
            .File(_rootPath, $"{solutionName}.sln")
            .ShouldExists();

        _asserts
            .File(_rootPath, "Project", $"{projectName}.csproj")
            .ShouldExists()
            .ShouldHaveContent(content);
    }
}