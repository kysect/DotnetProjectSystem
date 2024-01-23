using Kysect.DotnetProjectSystem.FileStructureBuilding;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.FileStructureBuilding;

public class ProjectFileStructureBuilderTests
{
    private readonly string _rootPath;
    private readonly MockFileSystem _fileSystem;

    public ProjectFileStructureBuilderTests()
    {
        _fileSystem = new MockFileSystem();
        _rootPath = _fileSystem.Path.GetFullPath(".");
    }

    [Fact]
    public void Save_EmptyProject_CreateCsprojFile()
    {
        string projectName = "Project1";
        string projectFileContent = "<Project></Project>";
        string expectedProjectPath = _fileSystem.Path.Combine(_rootPath, $"{projectName}", $"{projectName}.csproj");

        var projectFileStructureBuilder = new ProjectFileStructureBuilder(projectName, projectFileContent);
        projectFileStructureBuilder.Save(_fileSystem, _rootPath);

        _fileSystem.File.Exists(expectedProjectPath).Should().BeTrue();
        _fileSystem.File.ReadAllText(expectedProjectPath).Should().BeEquivalentTo(projectFileContent);
    }

    [Fact]
    public void Save_AfterAddingEmptyFile_CreateFile()
    {
        string projectName = "Project1";
        string projectFileContent = "<Project></Project>";
        string filePartialPath = "SomeFile.txt";
        string fileFullPath = _fileSystem.Path.Combine(_rootPath, $"{projectName}", filePartialPath);

        var projectFileStructureBuilder = new ProjectFileStructureBuilder(projectName, projectFileContent)
            .AddEmptyFile([filePartialPath]);

        projectFileStructureBuilder.Save(_fileSystem, _rootPath);

        _fileSystem.File.Exists(fileFullPath).Should().BeTrue();
        _fileSystem.File.ReadAllText(fileFullPath).Should().BeEmpty();
    }

    [Fact]
    public void Save_AfterAddingFileWithContent_CreateFileWithExpectedContent()
    {
        string projectName = "Project1";
        string projectFileContent = "<Project></Project>";
        string filePartialPath = "SomeFile.txt";
        string fileContent = "some text";
        string fileFullPath = _fileSystem.Path.Combine(_rootPath, $"{projectName}", filePartialPath);

        var projectFileStructureBuilder = new ProjectFileStructureBuilder(projectName, projectFileContent)
            .AddFile([filePartialPath], fileContent);

        projectFileStructureBuilder.Save(_fileSystem, _rootPath);

        _fileSystem.File.Exists(fileFullPath).Should().BeTrue();
        _fileSystem.File.ReadAllText(fileFullPath).Should().Be(fileContent);
    }

    [Fact]
    public void Save_AfterAddingFileToSubdirectory_CreateFile()
    {
        string projectName = "Project1";
        string projectFileContent = "<Project></Project>";
        string filePartialPath = "SomeFile.txt";
        var subdirectoryName = "Subdirectory";
        string fileFullPath = _fileSystem.Path.Combine(_rootPath, $"{projectName}", subdirectoryName, filePartialPath);

        var projectFileStructureBuilder = new ProjectFileStructureBuilder(projectName, projectFileContent)
            .AddEmptyFile([subdirectoryName, filePartialPath]);

        projectFileStructureBuilder.Save(_fileSystem, _rootPath);

        _fileSystem.File.Exists(fileFullPath).Should().BeTrue();
        _fileSystem.File.ReadAllText(fileFullPath).Should().BeEmpty();
    }
}