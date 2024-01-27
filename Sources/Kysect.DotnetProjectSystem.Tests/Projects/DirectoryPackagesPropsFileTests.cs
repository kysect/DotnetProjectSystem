using Kysect.DotnetProjectSystem.Projects;

namespace Kysect.DotnetProjectSystem.Tests.Projects;

public class DirectoryPackagesPropsFileTests
{
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
}