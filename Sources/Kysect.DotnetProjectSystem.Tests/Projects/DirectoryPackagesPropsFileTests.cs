using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Tests.Asserts;
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

    public DirectoryPackagesPropsFileTests()
    {
        _fileSystem = new MockFileSystem();
        var solutionFileContentParser = new SolutionFileContentParser();
        _solutionModifierFactory = new DotnetSolutionModifierFactory(_fileSystem, solutionFileContentParser);
        _currentPath = _fileSystem.Path.GetFullPath(".");
        _fileSystemAsserts = new FileSystemAsserts(_fileSystem);
        _syntaxFormatter = new XmlDocumentSyntaxFormatter();
    }

    [Fact]
    public void GetPackageVersions_ForFileWithVersion_ReturnExpectedResult()
    {
        const string input = """
                             <Project>
                               <PropertyGroup>
                                 <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                               </PropertyGroup>
                               <ItemGroup>
                                 <PackageVersion Include="First" Version="0.1.13" />
                                 <PackageVersion Include="Second" Version="0.1.12" />
                               </ItemGroup>
                             </Project>
                             """;
        IReadOnlyCollection<ProjectPackageVersion> expected =
        [
            new ProjectPackageVersion("First", "0.1.13"),
            new ProjectPackageVersion("Second", "0.1.12")
        ];

        var directoryPackagesPropsFile = new DirectoryPackagesPropsFile(DotnetProjectFile.Create(input));

        IReadOnlyCollection<ProjectPackageVersion> actual = directoryPackagesPropsFile.GetPackageVersions();

        actual.Should().BeEquivalentTo(expected);
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


        new SolutionFileStructureBuilder("Solution")
            .Save(_fileSystem, _currentPath, _syntaxFormatter);

        DotnetSolutionModifier solutionModifier = _solutionModifierFactory.Create("Solution.sln");
        solutionModifier
            .GetOrCreateDirectoryPackagePropsModifier()
            .SetCentralPackageManagement(true);
        solutionModifier.Save(_syntaxFormatter);

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