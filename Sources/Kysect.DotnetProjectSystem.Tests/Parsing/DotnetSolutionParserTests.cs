﻿using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tests.Asserts;
using Kysect.DotnetProjectSystem.Tests.Tools;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.Parsing;

public class DotnetSolutionParserTests
{
    private readonly MockFileSystem _fileSystem;
    private readonly DotnetSolutionParser _solutionStructureParser;

    private readonly string _currentPath;
    private readonly XmlDocumentSyntaxFormatter _syntaxFormatter;
    private readonly DotnetSolutionDescriptorAsserts _solutionDescriptorAsserts;
    private readonly SolutionFileStructureBuilderFactory _solutionFileStructureBuilderFactory;

    public DotnetSolutionParserTests()
    {
        ILogger logger = TestLoggerProvider.Provide();

        _fileSystem = new MockFileSystem();
        _solutionStructureParser = new DotnetSolutionParser(_fileSystem, logger);

        _currentPath = _fileSystem.Path.GetFullPath(".");
        _syntaxFormatter = new XmlDocumentSyntaxFormatter();
        _solutionDescriptorAsserts = new DotnetSolutionDescriptorAsserts(_syntaxFormatter);
        _solutionFileStructureBuilderFactory = new SolutionFileStructureBuilderFactory(_fileSystem, _syntaxFormatter);
    }

    [Fact]
    public void Parse_FileThatIsNotExist_ThrowException()
    {
        string solutionName = "Solution";
        string solutionPath = _fileSystem.Path.Combine(_currentPath, $"{solutionName}.sln");

        var exception = Assert.Throws<DotnetProjectSystemException>(() =>
        {
            _solutionStructureParser.Parse(solutionPath);
        });

        exception.Message.Should().Be($"Cannot parse solution {solutionPath}. File not found.");
    }

    [Fact]
    public void Parse_ForEmptySolution_ReturnInfoAboutSolution()
    {
        string solutionName = "Solution";
        string solutionPath = _fileSystem.Path.Combine(_currentPath, $"{solutionName}.sln");

        _solutionFileStructureBuilderFactory.Create(solutionName)
            .Save(_currentPath);

        DotnetSolutionDescriptor solutionDescriptor = _solutionStructureParser.Parse(solutionPath);

        solutionDescriptor.FilePath.Should().Be(solutionPath);
        solutionDescriptor.Projects.Should().HaveCount(0);
    }

    [Fact]
    public void Parse_SolutionWithProject_ReturnInformationAboutProject()
    {
        string projectContent = """
                                <Project Sdk="Microsoft.NET.Sdk">
                                  <PropertyGroup>
                                    <TargetFramework>net8.0</TargetFramework>
                                    <ImplicitUsings>enable</ImplicitUsings>
                                  </PropertyGroup>
                                </Project>
                                """;

        string solutionName = "Solution";
        string projectName = "ProjectName";
        string projectPath = _fileSystem.Path.Combine(_currentPath, projectName, $"{projectName}.csproj");
        string solutionFilePath = _fileSystem.Path.Combine(_currentPath, $"{solutionName}.sln");
        var expected = new DotnetSolutionDescriptor(
            solutionFilePath,
            new Dictionary<string, DotnetProjectFile> { { projectPath, DotnetProjectFile.Create(projectContent) } });

        _solutionFileStructureBuilderFactory.Create(solutionName)
            .AddProject(
                new ProjectFileStructureBuilder(projectName)
                    .SetContent(projectContent))
            .Save(_currentPath);

        DotnetSolutionDescriptor solutionDescriptor = _solutionStructureParser.Parse(solutionFilePath);

        _solutionDescriptorAsserts.Equals(solutionDescriptor, expected);
    }
}