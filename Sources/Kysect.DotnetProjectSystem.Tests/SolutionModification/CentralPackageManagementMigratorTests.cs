using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Tests.Asserts;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.SolutionModification;

public class CentralPackageManagementMigratorTests
{
    private readonly CentralPackageManagementMigrator _sut;
    private readonly DotnetSolutionModifierFactory _solutionModifierFactory;
    private readonly MockFileSystem _fileSystem;
    private readonly FileSystemAsserts _fileSystemAsserts;
    private readonly XmlDocumentSyntaxFormatter _formatter;
    private readonly string _currentPath;

    public CentralPackageManagementMigratorTests()
    {
        _formatter = new XmlDocumentSyntaxFormatter();
        _fileSystem = new MockFileSystem();
        _sut = new CentralPackageManagementMigrator(_formatter, DefaultLoggerConfiguration.CreateConsoleLogger());
        _solutionModifierFactory = new DotnetSolutionModifierFactory(_fileSystem, new SolutionFileContentParser());
        _currentPath = _fileSystem.Path.GetFullPath(".");
        _fileSystemAsserts = new FileSystemAsserts(_fileSystem);
    }

    [Fact]
    public void Migrate_SolutionWithCpm_ThrowException()
    {
        var directoryPackagesPropsFile = new DirectoryPackagesPropsFile(DotnetProjectFile.CreateEmpty());
        directoryPackagesPropsFile.SetCentralPackageManagement(true);

        new SolutionFileStructureBuilder("Solution")
            .AddDirectoryPackagesProps(directoryPackagesPropsFile.File.ToXmlString(_formatter))
            .Save(_fileSystem, _currentPath, _formatter);

        DotnetSolutionModifier solutionModifier = _solutionModifierFactory.Create("Solution.sln");
        Assert.Throws<DotnetProjectSystemException>(() =>
        {
            _sut.Migrate(solutionModifier);
        });
    }

    [Fact]
    public void Migrate_SolutionWithoutPackage_CreateDirectoryPackagesFile()
    {
        const string expected = """
                                <Project>
                                  <PropertyGroup>
                                    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                                  </PropertyGroup>
                                </Project>
                                """;

        new SolutionFileStructureBuilder("Solution")
            .Save(_fileSystem, _currentPath, _formatter);

        DotnetSolutionModifier solutionModifier = _solutionModifierFactory.Create("Solution.sln");
        _sut.Migrate(solutionModifier);

        _fileSystemAsserts
            .File([_currentPath, SolutionItemNameConstants.DirectoryPackagesProps])
            .ShouldExists()
            .ShouldHaveContent(expected);
    }

    [Fact]
    public void Migrate_ProjectWithPackage_MovePackageVersionToPackagesProps()
    {
        const string expectedProjectContent = """
                                              <Project>
                                                <ItemGroup>
                                                  <PackageReference Include="MyPackage" />
                                                </ItemGroup>
                                              </Project>
                                              """;

        const string expectedPackagesProps = """
                                             <Project>
                                               <PropertyGroup>
                                                 <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                                               </PropertyGroup>
                                               <ItemGroup>
                                                 <PackageVersion Include="MyPackage" Version="1.2.3" />
                                               </ItemGroup>
                                             </Project>
                                             """;

        var projectFile = DotnetProjectFile.CreateEmpty();
        projectFile.PackageReferences.AddPackageReference("MyPackage", "1.2.3");

        new SolutionFileStructureBuilder("Solution")
            .AddProject(
                new ProjectFileStructureBuilder("Project")
                    .SetContent(projectFile))
            .Save(_fileSystem, _currentPath, _formatter);

        DotnetSolutionModifier solutionModifier = _solutionModifierFactory.Create("Solution.sln");
        _sut.Migrate(solutionModifier);

        _fileSystemAsserts
            .File([_currentPath, SolutionItemNameConstants.DirectoryPackagesProps])
            .ShouldExists()
            .ShouldHaveContent(expectedPackagesProps);

        _fileSystemAsserts
            .File([_currentPath, "Project", "Project.csproj"])
            .ShouldExists()
            .ShouldHaveContent(expectedProjectContent);
    }
}