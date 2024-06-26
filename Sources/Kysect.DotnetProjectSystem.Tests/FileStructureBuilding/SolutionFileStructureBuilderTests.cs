﻿
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tests.Asserts;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.FileStructureBuilding;

public class SolutionFileStructureBuilderTests
{
    private readonly string _rootPath;
    private readonly MockFileSystem _fileSystem;
    private readonly FileSystemAsserts _asserts;
    private readonly XmlDocumentSyntaxFormatter _syntaxFormatter;
    private readonly SolutionFileStructureBuilderFactory _solutionFileStructureBuilderFactory;

    public SolutionFileStructureBuilderTests()
    {
        _fileSystem = new MockFileSystem();
        _rootPath = _fileSystem.Path.GetFullPath(".");
        _asserts = new FileSystemAsserts(_fileSystem);
        _syntaxFormatter = new XmlDocumentSyntaxFormatter();
        _solutionFileStructureBuilderFactory = new SolutionFileStructureBuilderFactory(_fileSystem, _syntaxFormatter);
    }

    [Fact]
    public void Save_ForEmptySolution_CreateSolutionFile()
    {
        string solutionName = "MySolution";

        _solutionFileStructureBuilderFactory.Create(solutionName)
            .Save(_rootPath);

        _asserts
            .File(_rootPath, $"{solutionName}.sln")
            .ShouldExists();
    }

    [Fact]
    public void Save_AfterAddingFile_FileShouldExists()
    {
        string solutionName = "MySolution";
        string content = "<Project></Project>";
        var directoryBuildPropsFile = new DirectoryBuildPropsFile(content);

        _solutionFileStructureBuilderFactory.Create(solutionName)
            .AddDirectoryBuildProps(directoryBuildPropsFile)
            .Save(_rootPath);

        _asserts
            .File(_rootPath, $"{solutionName}.sln")
            .ShouldExists();

        _asserts
            .File(_rootPath, SolutionItemNameConstants.DirectoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(content);
    }

    [Fact]
    public void Save_AfterAddingProject_ProjectFileShouldExists()
    {
        string solutionName = "MySolution";
        string projectName = "Project";
        string content = "<Project></Project>";

        _solutionFileStructureBuilderFactory.Create(solutionName)
            .AddProject(
                new ProjectFileStructureBuilder(projectName)
                    .SetContent(content))
            .Save(_rootPath);

        _asserts
            .File(_rootPath, $"{solutionName}.sln")
            .ShouldExists();

        _asserts
            .File(_rootPath, "Project", $"{projectName}.csproj")
            .ShouldExists()
            .ShouldHaveContent(content);
    }

    [Fact]
    public void Save_ParentDirectoryNotExist_CreateSolutionDirectory()
    {
        string solutionName = "MySolution";
        string solutionFullPath = _fileSystem.Path.Combine(_rootPath, "Sources");

        _solutionFileStructureBuilderFactory.Create(solutionName)
            .Save(solutionFullPath);

        _asserts
            .File(_rootPath, "Sources", $"{solutionName}.sln")
            .ShouldExists();
    }
}