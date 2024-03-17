using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Tests.Asserts;
using Kysect.DotnetProjectSystem.Tests.Tools;
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
    private readonly SolutionFileStructureBuilderFactory _solutionFileStructureBuilderFactory;

    public CentralPackageManagementMigratorTests()
    {
        _formatter = new XmlDocumentSyntaxFormatter();
        _fileSystem = new MockFileSystem();
        _sut = new CentralPackageManagementMigrator(TestLoggerProvider.Provide());
        _solutionModifierFactory = new DotnetSolutionModifierFactory(_fileSystem, new SolutionFileContentParser(), _formatter);
        _currentPath = _fileSystem.Path.GetFullPath(".");
        _fileSystemAsserts = new FileSystemAsserts(_fileSystem);
        _solutionFileStructureBuilderFactory = new SolutionFileStructureBuilderFactory(_fileSystem, _formatter);
    }

    [Fact]
    public void Migrate_SolutionWithCpm_ThrowException()
    {
        var directoryPackagesPropsFile = new DirectoryPackagesPropsFile(DotnetProjectFile.CreateEmpty());
        directoryPackagesPropsFile.SetCentralPackageManagement(true);

        _solutionFileStructureBuilderFactory.Create("Solution")
            .AddDirectoryPackagesProps(directoryPackagesPropsFile)
            .Save(_currentPath);

        DotnetSolutionModifier solutionModifier = _solutionModifierFactory.Create("Solution.sln");
        var exception = Assert.Throws<DotnetProjectSystemException>(() =>
        {
            _sut.Migrate(solutionModifier);
        });

        exception.Message.Should().Be("Cannot migrate solution to CPM. Solution already use CPM.");
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

        _solutionFileStructureBuilderFactory.Create("Solution")
            .Save(_currentPath);

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

        _solutionFileStructureBuilderFactory.Create("Solution")
            .AddProject(
                new ProjectFileStructureBuilder("Project")
                    .SetContent(projectFile))
            .Save(_currentPath);

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

    [Fact]
    public void Migrate_TwoProjectWithSamePackage_MovePackageVersionToPackagesPropsWithoutDuplication()
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

        _solutionFileStructureBuilderFactory.Create("Solution")
            .AddProject(
                new ProjectFileStructureBuilder("Project")
                    .SetContent(projectFile))
            .AddProject(
                new ProjectFileStructureBuilder("Project2")
                    .SetContent(projectFile))
            .Save(_currentPath);

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

        _fileSystemAsserts
            .File([_currentPath, "Project2", "Project2.csproj"])
            .ShouldExists()
            .ShouldHaveContent(expectedProjectContent);
    }

    [Fact]
    public void Migrate_TwoProjectWithPackageWithDifferentVersion_PackagesPropsContainsMaxVersion()
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

        _solutionFileStructureBuilderFactory.Create("Solution")
            .AddProject(
                new ProjectFileStructureBuilder("Project")
                    .SetContent(DotnetProjectFile.CreateEmpty().PackageReferences.AddPackageReference("MyPackage", "1.2.2")))
            .AddProject(
                new ProjectFileStructureBuilder("Project2")
                    .SetContent(DotnetProjectFile.CreateEmpty().PackageReferences.AddPackageReference("MyPackage", "1.2.3")))
            .Save(_currentPath);

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

        _fileSystemAsserts
            .File([_currentPath, "Project2", "Project2.csproj"])
            .ShouldExists()
            .ShouldHaveContent(expectedProjectContent);
    }
}