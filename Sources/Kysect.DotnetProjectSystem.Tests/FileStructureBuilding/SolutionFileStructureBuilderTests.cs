using Kysect.DotnetProjectSystem.FileStructureBuilding;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.FileStructureBuilding;

public class SolutionFileStructureBuilderTests
{
    private readonly string _rootPath;
    private readonly MockFileSystem _fileSystem;

    public SolutionFileStructureBuilderTests()
    {
        _fileSystem = new MockFileSystem();
        _rootPath = _fileSystem.Path.GetFullPath(".");
    }

    [Fact]
    public void Save_ForEmptySolution_CreateSolutionFile()
    {
        string solutionName = "MySolution";

        new SolutionFileStructureBuilder(solutionName)
            .Save(_fileSystem, _rootPath);

        var expectedSolutionPath = _fileSystem.Path.Combine(_rootPath, $"{solutionName}.sln");
        _fileSystem.File.Exists(expectedSolutionPath).Should().BeTrue();
    }

    [Fact]
    public void Save_AfterAddingFile_FileShouldExists()
    {
        string solutionName = "MySolution";
        string directoryBuildProps = "Directory.Build.props";
        string content = "<Project></Project>";

        new SolutionFileStructureBuilder(solutionName)
            .AddFile([directoryBuildProps], content)
            .Save(_fileSystem, _rootPath);

        var expectedSolutionPath = _fileSystem.Path.Combine(_rootPath, $"{solutionName}.sln");
        var expectedDirectoryBuildPropsPath = _fileSystem.Path.Combine(_rootPath, directoryBuildProps);
        _fileSystem.File.Exists(expectedSolutionPath).Should().BeTrue();
        _fileSystem.File.Exists(expectedDirectoryBuildPropsPath).Should().BeTrue();
        _fileSystem.File.ReadAllText(expectedDirectoryBuildPropsPath).Should().BeEquivalentTo(content);
    }

    [Fact]
    public void Save_AfterAddingProject_ProjectFileShouldExists()
    {
        string solutionName = "MySolution";
        string projectName = "Project";
        string content = "<Project></Project>";

        new SolutionFileStructureBuilder(solutionName)
            .AddProject(new ProjectFileStructureBuilder(projectName, content))
            .Save(_fileSystem, _rootPath);

        var expectedSolutionPath = _fileSystem.Path.Combine(_rootPath, $"{solutionName}.sln");
        var expectedProjectPath = _fileSystem.Path.Combine(_rootPath, "Project", $"{projectName}.csproj");

        _fileSystem.File.Exists(expectedSolutionPath).Should().BeTrue();
        _fileSystem.File.Exists(expectedProjectPath).Should().BeTrue();
        _fileSystem.File.ReadAllText(expectedProjectPath).Should().BeEquivalentTo(content);
    }
}