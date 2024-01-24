using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.Parsing;

public class DotnetSolutionParserTests
{
    private readonly MockFileSystem _fileSystem;
    private readonly DotnetSolutionParser _solutionStructureParser;

    private readonly string _currentPath;
    private readonly XmlDocumentSyntaxFormatter _xmlDocumentSyntaxFormatter;

    public DotnetSolutionParserTests()
    {
        ILogger logger = DefaultLoggerConfiguration.CreateConsoleLogger();

        _fileSystem = new MockFileSystem();
        _solutionStructureParser = new DotnetSolutionParser(_fileSystem, logger);

        _currentPath = _fileSystem.Path.GetFullPath(".");
        _xmlDocumentSyntaxFormatter = new XmlDocumentSyntaxFormatter();
    }

    [Fact]
    public void Parse_ForEmptySolution_ReturnInfoAboutSolution()
    {
        string solutionName = "Solution";
        string solutionPath = _fileSystem.Path.Combine(_currentPath, $"{solutionName}.sln");
        new SolutionFileStructureBuilder(solutionName)
            .Save(_fileSystem, _currentPath);

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
                                    <Nullable>enable</Nullable>
                                  </PropertyGroup>
                                
                                  <ItemGroup>
                                    <PackageReference Include="FluentAssertions" />
                                    <PackageReference Include="Microsoft.NET.Test.Sdk" />
                                  </ItemGroup>
                                </Project>
                                """;

        string solutionName = "Solution";
        string projectName = "ProjectName";
        string projectPath = _fileSystem.Path.Combine(_currentPath, projectName, $"{projectName}.csproj");

        string solutionFilePath = _fileSystem.Path.Combine(_currentPath, $"{solutionName}.sln");
        new SolutionFileStructureBuilder(solutionName)
            .AddProject(new ProjectFileStructureBuilder(projectName, projectContent))
            .Save(_fileSystem, _currentPath);

        DotnetSolutionDescriptor solutionDescriptor = _solutionStructureParser.Parse(solutionFilePath);
        solutionDescriptor.Projects.Should().HaveCount(1);

        (string? actualProjectPath, DotnetProjectFile? actualProjectContent) = solutionDescriptor.Projects.Single();
        actualProjectPath.Should().Be(projectPath);
        actualProjectContent.ToXmlString(_xmlDocumentSyntaxFormatter).Should().Be(projectContent);
    }
}