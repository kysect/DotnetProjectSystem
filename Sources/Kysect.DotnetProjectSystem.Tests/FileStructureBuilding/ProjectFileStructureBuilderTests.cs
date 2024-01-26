using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Tests.Asserts;
using Kysect.DotnetProjectSystem.Xml;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.FileStructureBuilding;

public class ProjectFileStructureBuilderTests
{
    private readonly string _rootPath;
    private readonly MockFileSystem _fileSystem;
    private readonly FileSystemAsserts _asserts;
    private readonly XmlDocumentSyntaxFormatter _xmlDocumentSyntaxFormatter;

    public ProjectFileStructureBuilderTests()
    {
        _fileSystem = new MockFileSystem();
        _rootPath = _fileSystem.Path.GetFullPath(".");
        _xmlDocumentSyntaxFormatter = new XmlDocumentSyntaxFormatter();
        _asserts = new FileSystemAsserts(_fileSystem);
    }

    [Fact]
    public void Save_EmptyProject_CreateCsprojFile()
    {
        string projectName = "Project1";
        string projectFileContent = "<Project></Project>";

        new ProjectFileStructureBuilder(projectName)
            .SetContent(projectFileContent)
            .Save(_fileSystem, _rootPath, _xmlDocumentSyntaxFormatter);

        _asserts
            .File(_rootPath, $"{projectName}", $"{projectName}.csproj")
            .ShouldExists()
            .ShouldHaveContent(projectFileContent);
    }

    [Fact]
    public void Save_AfterAddingEmptyFile_CreateFile()
    {
        string projectName = "Project1";
        string projectFileContent = "<Project></Project>";
        string filePartialPath = "SomeFile.txt";
        string fileFullPath = _fileSystem.Path.Combine(_rootPath, $"{projectName}", filePartialPath);

        new ProjectFileStructureBuilder(projectName)
            .SetContent(projectFileContent)
            .AddEmptyFile(filePartialPath)
            .Save(_fileSystem, _rootPath, _xmlDocumentSyntaxFormatter);

        _asserts
            .File(fileFullPath)
            .ShouldExists()
            .ShouldHaveEmptyContent();
    }

    [Fact]
    public void Save_AfterAddingFileWithContent_CreateFileWithExpectedContent()
    {
        string projectName = "Project1";
        string projectFileContent = "<Project></Project>";
        string filePartialPath = "SomeFile.txt";
        string fileContent = "some text";
        string fileFullPath = _fileSystem.Path.Combine(_rootPath, $"{projectName}", filePartialPath);

        new ProjectFileStructureBuilder(projectName)
            .SetContent(projectFileContent)
            .AddFile([filePartialPath], fileContent)
            .Save(_fileSystem, _rootPath, _xmlDocumentSyntaxFormatter);

        _asserts
            .File(fileFullPath)
            .ShouldExists()
            .ShouldHaveContent(fileContent);
    }

    [Fact]
    public void Save_AfterAddingFileToSubdirectory_CreateFile()
    {
        string projectName = "Project1";
        string projectFileContent = "<Project></Project>";
        string filePartialPath = "SomeFile.txt";
        string subdirectoryName = "Subdirectory";
        string fileFullPath = _fileSystem.Path.Combine(_rootPath, $"{projectName}", subdirectoryName, filePartialPath);

        new ProjectFileStructureBuilder(projectName)
            .SetContent(projectFileContent)
            .AddEmptyFile(subdirectoryName, filePartialPath)
            .Save(_fileSystem, _rootPath, _xmlDocumentSyntaxFormatter);

        _asserts
            .File(fileFullPath)
            .ShouldExists()
            .ShouldHaveEmptyContent();
    }
}