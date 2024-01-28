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

    public DotnetSolutionModifierFactoryTests()
    {
        _fileSystem = new MockFileSystem();
        var solutionFileContentParser = new SolutionFileContentParser();
        _solutionModifierFactory = new DotnetSolutionModifierFactory(_fileSystem, solutionFileContentParser);
        _syntaxFormatter = new XmlDocumentSyntaxFormatter();

        _currentPath = _fileSystem.Path.GetFullPath(".");
    }

    [Fact]
    public void Create_ForSolutionWithIncorrectProjectPath_ThrowException()
    {
        new SolutionFileStructureBuilder("Solution")
            .AddProject(new ProjectFileStructureBuilder("Project"))
            .Save(_fileSystem, _currentPath, _syntaxFormatter);

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

        new SolutionFileStructureBuilder("Solution")
            .AddDirectoryBuildProps(directoryBuildPropsContent)
            .AddDirectoryPackagesProps(directoryPackagesPropsContent)
            .Save(_fileSystem, _currentPath, _syntaxFormatter);

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

        new SolutionFileStructureBuilder("Solution")
            .Save(_fileSystem, _currentPath, _syntaxFormatter);

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