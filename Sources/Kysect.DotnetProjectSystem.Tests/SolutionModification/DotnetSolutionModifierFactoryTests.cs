using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.SolutionModification;

public class DotnetSolutionModifierFactoryTests
{
    private readonly MockFileSystem _fileSystem;
    private readonly DotnetSolutionModifierFactory _solutionModifierFactory;
    private readonly XmlDocumentSyntaxFormatter _syntaxFormatter;
    private readonly string _currentPath;
    private readonly SolutionFileStructureBuilderFactory _solutionFileStructureBuilderFactory;

    public DotnetSolutionModifierFactoryTests()
    {
        _fileSystem = new MockFileSystem();
        _syntaxFormatter = new XmlDocumentSyntaxFormatter();
        var solutionFileContentParser = new SolutionFileContentParser();
        _solutionModifierFactory = new DotnetSolutionModifierFactory(_fileSystem, solutionFileContentParser, _syntaxFormatter);
        _solutionFileStructureBuilderFactory = new SolutionFileStructureBuilderFactory(_fileSystem, _syntaxFormatter);

        _currentPath = _fileSystem.Path.GetFullPath(".");
    }

    [Fact]
    public void Create_ForSolutionWithIncorrectProjectPath_ThrowException()
    {
        _solutionFileStructureBuilderFactory.Create("Solution")
            .AddProject(new ProjectFileStructureBuilder("Project"))
            .Save(_currentPath);

        string projectFilePath = _fileSystem.Path.Combine(_currentPath, "Project", "Project.csproj");
        _fileSystem.File.Delete(projectFilePath);

        var exception = Assert.Throws<DotnetProjectSystemException>(() =>
        {
            _solutionModifierFactory.Create("Solution.sln");
        });

        exception.Message.Should().Be($"Project file with path {projectFilePath} was not found");
    }

    [Fact]
    public void Create_ForSolutionWithDirectoryFiles_CreateModifierWithData()
    {
        var directoryBuildPropsContent = """
                                         <Project>
                                           <PropertyGroup>
                                           </PropertyGroup>
                                         </Project>
                                         """;

        var directoryPackagesPropsContent = """
                                            <Project>
                                              <PropertyGroup>
                                              </PropertyGroup>
                                            </Project>
                                            """;

        _solutionFileStructureBuilderFactory.Create("Solution")
            .AddDirectoryBuildProps(directoryBuildPropsContent)
            .AddDirectoryPackagesProps(directoryPackagesPropsContent)
            .Save(_currentPath);

        DotnetSolutionModifier solutionModifier = _solutionModifierFactory.Create("Solution.sln");
        solutionModifier
            .GetOrCreateDirectoryBuildPropsModifier()
            .File
            .ToXmlString(_syntaxFormatter)
            .Should().Be(directoryBuildPropsContent);

        solutionModifier
            .GetOrCreateDirectoryPackagePropsModifier()
            .File
            .ToXmlString(_syntaxFormatter)
            .Should().Be(directoryPackagesPropsContent);
    }

    [Fact]
    public void Create_ForSolutionWithoutDirectoryFiles_DirectoryFileMustBeCreatedOnRead()
    {
        var directoryBuildPropsContent = """
                                         <Project>
                                         </Project>
                                         """;

        var directoryPackagesPropsContent = """
                                            <Project>
                                            </Project>
                                            """;

        _solutionFileStructureBuilderFactory.Create("Solution")
            .Save(_currentPath);

        DotnetSolutionModifier solutionModifier = _solutionModifierFactory.Create("Solution.sln");
        solutionModifier
            .GetOrCreateDirectoryBuildPropsModifier()
            .File
            .ToXmlString(_syntaxFormatter)
            .Should().Be(directoryBuildPropsContent);

        solutionModifier
            .GetOrCreateDirectoryPackagePropsModifier()
            .File
            .ToXmlString(_syntaxFormatter)
            .Should().Be(directoryPackagesPropsContent);
    }
}