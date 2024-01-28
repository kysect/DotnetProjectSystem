using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Xml;

namespace Kysect.DotnetProjectSystem.Tests.Projects;

public class DotnetProjectFilePackageReferencesTests
{
    private readonly XmlDocumentSyntaxFormatter _formatter;

    public DotnetProjectFilePackageReferencesTests()
    {
        _formatter = new XmlDocumentSyntaxFormatter();
    }

    [Fact]
    public void GetPackageReferences_PackageWithoutVersion_ReturnExpectedResult()
    {
        const string input = """
                             <Project>
                               <ItemGroup>
                                 <PackageReference Include="PackageName">
                               </ItemGroup>
                             </Project>
                             """;

        IReadOnlyCollection<ProjectPackageReference> expected = [new ProjectPackageReference("PackageName", null)];

        IReadOnlyCollection<ProjectPackageReference> references = DotnetProjectFile
            .Create(input)
            .PackageReferences
            .GetPackageReferences();

        references.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetPackageReferences_PackageWithVersion_ReturnExpectedResult()
    {
        const string input = """
                             <Project>
                               <ItemGroup>
                                 <PackageReference Include="PackageName" Version="1.0.0">
                               </ItemGroup>
                             </Project>
                             """;

        IReadOnlyCollection<ProjectPackageReference> expected = [new ProjectPackageReference("PackageName", "1.0.0")];

        IReadOnlyCollection<ProjectPackageReference> references = DotnetProjectFile
            .Create(input)
            .PackageReferences
            .GetPackageReferences();

        references.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void AddPackageReference_PackageWithoutVersion_AddRequiredString()
    {
        var expected = """
                       <Project>
                         <ItemGroup>
                           <PackageReference Include="PackageName" />
                         </ItemGroup>
                       </Project>
                       """;

        var projectFile = DotnetProjectFile.CreateEmpty();

        projectFile.PackageReferences.AddPackageReference("PackageName");

        projectFile.ToXmlString(_formatter).Should().Be(expected);
    }

    [Fact]
    public void AddPackageReference_PackageWithVersion_AddRequiredString()
    {
        var expected = """
                       <Project>
                         <ItemGroup>
                           <PackageReference Include="PackageName" Version="1.2.3" />
                         </ItemGroup>
                       </Project>
                       """;

        var projectFile = DotnetProjectFile.CreateEmpty();

        projectFile.PackageReferences.AddPackageReference("PackageName", "1.2.3");

        projectFile.ToXmlString(_formatter).Should().Be(expected);
    }

    [Fact]
    public void RemovePackageReference_ProjectWithPackage_PackageRemoved()
    {
        var input = """
                    <Project>
                      <ItemGroup>
                        <PackageReference Include="PackageName" Version="1.2.3" />
                        <PackageReference Include="PackageName1" Version="1.2.3" />
                      </ItemGroup>
                    </Project>
                    """;

        var expected = """
                       <Project>
                         <ItemGroup>
                           <PackageReference Include="PackageName1" Version="1.2.3" />
                         </ItemGroup>
                       </Project>
                       """;

        var projectFile = DotnetProjectFile.Create(input);

        projectFile.PackageReferences.RemovePackageReference("PackageName");

        projectFile.ToXmlString(_formatter).Should().Be(expected);
    }

    [Fact]
    public void UpdatePackageReference_ProjectWithVersion_ProjectHaveChangedVersion()
    {
        var input = """
                    <Project>
                      <ItemGroup>
                        <PackageReference Include="PackageName" Version="1.2.3" />
                      </ItemGroup>
                    </Project>
                    """;

        var expected = """
                       <Project>
                         <ItemGroup>
                           <PackageReference Include="PackageName" Version="1.2.4" />
                         </ItemGroup>
                       </Project>
                       """;

        var projectFile = DotnetProjectFile.Create(input);

        projectFile.PackageReferences.SetPackageReference("PackageName", "1.2.4");

        projectFile.ToXmlString(_formatter).Should().Be(expected);
    }

    [Fact]
    public void RemoveVersion_ProjectWithVersion_ReturnExpectedResult()
    {
        var input = """
                    <Project>
                      <ItemGroup>
                        <PackageReference Include="PackageName" Version="1.2.0" />
                        <PackageReference Include="PackageName2" Version="1.2.3" />
                      </ItemGroup>
                    </Project>
                    """;

        var expected = """
                       <Project>
                         <ItemGroup>
                           <PackageReference Include="PackageName" />
                           <PackageReference Include="PackageName2" Version="1.2.3" />
                         </ItemGroup>
                       </Project>
                       """;

        var projectFile = DotnetProjectFile.Create(input);

        projectFile.PackageReferences.RemoveVersion("PackageName");

        projectFile.ToXmlString(_formatter).Should().Be(expected);
    }
}