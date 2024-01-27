using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Tests.Asserts;
using Kysect.DotnetProjectSystem.Xml;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.SolutionModification;

public class DotnetSolutionModifierTests
{
    private readonly MockFileSystem _fileSystem;
    private readonly XmlDocumentSyntaxFormatter _syntaxFormatter;
    private readonly DotnetSolutionModifierFactory _solutionModifierFactory;
    private readonly FileSystemAsserts _fileSystemAsserts;
    private readonly string _currentPath;

    public DotnetSolutionModifierTests()
    {
        _fileSystem = new MockFileSystem();
        var solutionFileContentParser = new SolutionFileContentParser();
        _solutionModifierFactory = new DotnetSolutionModifierFactory(_fileSystem, solutionFileContentParser);
        _currentPath = _fileSystem.Path.GetFullPath(".");
        _fileSystemAsserts = new FileSystemAsserts(_fileSystem);
        _syntaxFormatter = new XmlDocumentSyntaxFormatter();
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

        new SolutionFileStructureBuilder("Solution")
            .AddProject(
                new ProjectFileStructureBuilder("SampleProject")
                    .SetContent(projectContent))
            .Save(_fileSystem, _currentPath, _syntaxFormatter);

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

        new SolutionFileStructureBuilder("Solution")
            .AddProject(
                new ProjectFileStructureBuilder("SampleProject")
                    .SetContent(projectContent))
            .Save(_fileSystem, _currentPath, _syntaxFormatter);

        DotnetSolutionModifier solutionModifier = _solutionModifierFactory.Create("Solution.sln");

        foreach (DotnetProjectModifier solutionModifierProject in solutionModifier.Projects)
            solutionModifierProject.File.UpdateDocument(new SetTargetFrameworkModifyStrategy("net9.0"));

        solutionModifier.Save(_syntaxFormatter);

        _fileSystemAsserts
            .File("SampleProject", "SampleProject.csproj")
            .ShouldExists()
            .ShouldHaveContent(expectedProjectContent);
    }

    [Fact]
    public void Save_AfterAddingValueToDirectoryBuildProps_FileSaved()
    {
        var expectedContent = """
                              <Project>
                                <PropertyGroup>
                                  <Key>value</Key>
                                </PropertyGroup>
                              </Project>
                              """;

        new SolutionFileStructureBuilder("Solution")
            .Save(_fileSystem, _currentPath, _syntaxFormatter);

        DotnetSolutionModifier solutionModifier = _solutionModifierFactory.Create("Solution.sln");
        solutionModifier
            .GetOrCreateDirectoryBuildPropsModifier()
            .File
            .AddProperty("Key", "value");
        solutionModifier.Save(_syntaxFormatter);

        _fileSystemAsserts
            .File(SolutionItemNameConstants.DirectoryBuildProps)
            .ShouldExists()
            .ShouldHaveContent(expectedContent);
    }
}