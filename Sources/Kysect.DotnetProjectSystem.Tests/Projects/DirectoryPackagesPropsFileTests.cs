using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Tests.Asserts;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.Projects;

public class DirectoryPackagesPropsFileTests
{
    private readonly MockFileSystem _fileSystem;
    private readonly XmlDocumentSyntaxFormatter _syntaxFormatter;
    private readonly DotnetSolutionModifierFactory _solutionModifierFactory;
    private readonly FileSystemAsserts _fileSystemAsserts;
    private readonly string _currentPath;
    private readonly SolutionFileStructureBuilderFactory _solutionFileStructureBuilderFactory;

    public DirectoryPackagesPropsFileTests()
    {
        _fileSystem = new MockFileSystem();
        _syntaxFormatter = new XmlDocumentSyntaxFormatter();
        var solutionFileContentParser = new SolutionFileContentParser();
        _solutionModifierFactory = new DotnetSolutionModifierFactory(_fileSystem, solutionFileContentParser, _syntaxFormatter);
        _currentPath = _fileSystem.Path.GetFullPath(".");
        _fileSystemAsserts = new FileSystemAsserts(_fileSystem);
        _solutionFileStructureBuilderFactory = new SolutionFileStructureBuilderFactory(_fileSystem, _syntaxFormatter);
    }

    [Fact]
    public void SetCentralPackageManagement_ForEmptyFile_ReturnExpectedResult()
    {
        var expectedContent = """
                              <Project>
                                <PropertyGroup>
                                  <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                                </PropertyGroup>
                              </Project>
                              """;


        _solutionFileStructureBuilderFactory
            .Create("Solution")
            .Save(_currentPath);

        DotnetSolutionModifier solutionModifier = _solutionModifierFactory.Create("Solution.sln");
        solutionModifier
            .GetOrCreateDirectoryPackagePropsModifier()
            .SetCentralPackageManagement(true);
        solutionModifier.Save();

        _fileSystemAsserts
            .File(SolutionItemNameConstants.DirectoryPackagesProps)
            .ShouldExists()
            .ShouldHaveContent(expectedContent);
    }

    [Fact]
    public void GetCentralPackageManagement_EnabledCpm_ReturnExpectedResult()
    {
        var expectedContent = """
                              <Project>
                                <PropertyGroup>
                                  <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                                </PropertyGroup>
                              </Project>
                              """;

        var directoryPackagesPropsFile = new DirectoryPackagesPropsFile(DotnetProjectFile.Create(expectedContent));

        directoryPackagesPropsFile.GetCentralPackageManagement().Should().BeTrue();
    }
}