using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Xml;

namespace Kysect.DotnetProjectSystem.Tests.Projects;

public class DirectoryPackagesPropsVersionsTests
{
    private readonly XmlDocumentSyntaxFormatter _syntaxFormatter;

    public DirectoryPackagesPropsVersionsTests()
    {
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

        IReadOnlyCollection<ProjectPackageVersion> actual = directoryPackagesPropsFile.Versions.GetPackageVersions();

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void AddPackageVersion_ForEmptyFile_ReturnExpectedResult()
    {
        const string input = """
                             <Project>
                               <PropertyGroup>
                                 <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                               </PropertyGroup>
                               <ItemGroup>
                                 <PackageVersion Include="First" Version="0.1.13" />
                               </ItemGroup>
                             </Project>
                             """;

        const string expected = """
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

        var directoryPackagesPropsFile = new DirectoryPackagesPropsFile(DotnetProjectFile.Create(input));

        directoryPackagesPropsFile.Versions.AddPackageVersion("Second", "0.1.12");

        directoryPackagesPropsFile.File.ToXmlString(_syntaxFormatter).Should().Be(expected);
    }

    [Fact]
    public void RemovePackageVersion_ForFileWithVersion_ReturnExpectedResult()
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

        const string expected = """
                                <Project>
                                  <PropertyGroup>
                                    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                                  </PropertyGroup>
                                  <ItemGroup>
                                    <PackageVersion Include="First" Version="0.1.13" />
                                  </ItemGroup>
                                </Project>
                                """;

        var directoryPackagesPropsFile = new DirectoryPackagesPropsFile(DotnetProjectFile.Create(input));

        directoryPackagesPropsFile.Versions.RemovePackageVersion("Second");

        directoryPackagesPropsFile.File.ToXmlString(_syntaxFormatter).Should().Be(expected);
    }

    [Fact]
    public void SetPackageVersion_ForFileWithVersion_ReturnExpectedResult()
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

        const string expected = """
                                <Project>
                                  <PropertyGroup>
                                    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                                  </PropertyGroup>
                                  <ItemGroup>
                                    <PackageVersion Include="First" Version="0.1.14" />
                                    <PackageVersion Include="Second" Version="0.1.12" />
                                  </ItemGroup>
                                </Project>
                                """;

        var directoryPackagesPropsFile = new DirectoryPackagesPropsFile(DotnetProjectFile.Create(input));

        directoryPackagesPropsFile.Versions.SetPackageVersion("First", "0.1.14");

        directoryPackagesPropsFile.File.ToXmlString(_syntaxFormatter).Should().Be(expected);
    }
}