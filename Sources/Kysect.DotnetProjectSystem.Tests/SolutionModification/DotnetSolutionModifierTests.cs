using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.SolutionModification;

public class DotnetSolutionModifierTests
{
    private readonly MockFileSystem _fileSystem;
    private readonly XmlDocumentSyntaxFormatter _syntaxFormatter;
    private readonly DotnetSolutionModifierFactory _solutionModifierFactory;

    public DotnetSolutionModifierTests()
    {
        var solutionFileContentParser = new SolutionFileContentParser();
        DefaultLoggerConfiguration.CreateConsoleLogger();
        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

        _syntaxFormatter = new XmlDocumentSyntaxFormatter();
        _solutionModifierFactory = new DotnetSolutionModifierFactory(_fileSystem, solutionFileContentParser);
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

        var solutionModifier = _solutionModifierFactory.Create("Solution.sln");
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

        var solutionModifier = _solutionModifierFactory.Create("Solution.sln");

        foreach (DotnetProjectModifier solutionModifierProject in solutionModifier.Projects)
            solutionModifierProject.File.UpdateDocument(new SetTargetFrameworkModifyStrategy("net9.0"));

        solutionModifier.Save(_syntaxFormatter);

        string fullPathToProjectFile = Path.Combine(@"SampleProject", "SampleProject.csproj");
        _fileSystem.File.ReadAllText(fullPathToProjectFile).Should().Be(expectedProjectContent);
    }
}