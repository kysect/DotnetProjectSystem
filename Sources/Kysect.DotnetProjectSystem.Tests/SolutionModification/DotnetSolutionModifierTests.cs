using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.SolutionModification;


public class DotnetSolutionModifierTests
{
    private readonly MockFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly XmlDocumentSyntaxFormatter _syntaxFormatter;

    public DotnetSolutionModifierTests()
    {
        _logger = DefaultLoggerConfiguration.CreateConsoleLogger();
        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

        _syntaxFormatter = new XmlDocumentSyntaxFormatter();
    }

    [Fact]
    public void CreateModifier_ReturnFullPathToProjects()
    {
        string projectContent = """
                                <Project Sdk="Microsoft.NET.Sdk">
                                  <PropertyGroup>
                                    <TargetFramework>net8.0</TargetFramework>
                                  </PropertyGroup>
                                </Project>
                                """;

        string projectName = "SampleProject";
        string solutionSln = "Solution.sln";
        string currentPath = _fileSystem.Path.GetFullPath("SolutionDirectory");
        string solutionPath = _fileSystem.Path.Combine(currentPath, solutionSln);

        _fileSystem.Directory.CreateDirectory(currentPath);

        var solutionBuilder = new SolutionFileStructureBuilder("Solution")
            .AddProject(new ProjectFileStructureBuilder(projectName, projectContent));
        solutionBuilder.Save(_fileSystem, currentPath, _syntaxFormatter);

        var solutionModifier = DotnetSolutionModifier.Create(solutionPath, _fileSystem, _logger, new SolutionFileContentParser());

        solutionModifier.Projects.Single().Path.Should().Be(_fileSystem.Path.Combine(currentPath, projectName, $"{projectName}.csproj"));
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

        string currentPath = _fileSystem.Path.GetFullPath(".");

        var solutionBuilder = new SolutionFileStructureBuilder("Solution")
            .AddProject(
                new ProjectFileStructureBuilder("SampleProject", projectContent));
        solutionBuilder.Save(_fileSystem, currentPath, _syntaxFormatter);

        var solutionModifier = DotnetSolutionModifier.Create("Solution.sln", _fileSystem, _logger, new SolutionFileContentParser());
        solutionModifier.Save(_syntaxFormatter);
    }

    [Fact]
    public void Save_AfterChangingTargetFramework_ChangeFileContentToExpected()
    {
        string projectContent = """
                                <Project Sdk="Microsoft.NET.Sdk">
                                  <PropertyGroup>
                                    <TargetFramework>net8.0</TargetFramework>
                                  </PropertyGroup>
                                </Project>
                                """;

        var expectedProjectContent = """
                                     <Project Sdk="Microsoft.NET.Sdk">
                                       <PropertyGroup>
                                         <TargetFramework>net9.0</TargetFramework>
                                       </PropertyGroup>
                                     </Project>
                                     """;

        string currentPath = _fileSystem.Path.GetFullPath(".");
        var solutionBuilder = new SolutionFileStructureBuilder("Solution")
            .AddProject(
                new ProjectFileStructureBuilder("SampleProject", projectContent));
        solutionBuilder.Save(_fileSystem, currentPath, _syntaxFormatter);

        var solutionModifier = DotnetSolutionModifier.Create("Solution.sln", _fileSystem, _logger, new SolutionFileContentParser());

        foreach (DotnetProjectModifier solutionModifierProject in solutionModifier.Projects)
            solutionModifierProject.Accessor.UpdateDocument(new SetTargetFrameworkModifyStrategy("net9.0"));

        solutionModifier.Save(_syntaxFormatter);

        string fullPathToProjectFile = Path.Combine(@"SampleProject", "SampleProject.csproj");
        _fileSystem.File.ReadAllText(fullPathToProjectFile).Should().Be(expectedProjectContent);
    }
}