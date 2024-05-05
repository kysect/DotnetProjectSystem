using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;

namespace Kysect.DotnetProjectSystem.Tests.Projects;

public class DotnetProjectFilePropertiesTests
{
    private readonly XmlDocumentSyntaxFormatter _formatter = new();

    [Fact]
    public void GetProperties_EmptyFile_ReturnEmptyCollection()
    {
        var sut = DotnetProjectFile.CreateEmpty();

        IReadOnlyCollection<DotnetProjectProperty> values = sut.Properties.GetProperties("Property");

        values.Should().BeEmpty();
    }

    [Fact]
    public void GetProperties_OneProperty_ReturnCollectionWithOneElement()
    {
        const string content = """
                               <Project>
                                 <PropertyGroup>
                                   <Property>Value</Property>
                                 </PropertyGroup>
                               </Project>
                               """;
        var sut = DotnetProjectFile.Create(content);

        IReadOnlyCollection<DotnetProjectProperty> values = sut.Properties.GetProperties("Property");

        values.Should().HaveCount(1);
    }

    [Fact]
    public void GetProperties_TwoProperty_ReturnExpectedCollection()
    {
        var expected = new DotnetProjectProperty[]
        {
            new DotnetProjectProperty("Property", "Value1"),
            new DotnetProjectProperty("Property", "Value2"),
        };
        const string content = """
                               <Project>
                                 <PropertyGroup>
                                   <Property>Value1</Property>
                                   <Property>Value2</Property>
                                 </PropertyGroup>
                               </Project>
                               """;
        var sut = DotnetProjectFile.Create(content);

        IReadOnlyCollection<DotnetProjectProperty> values = sut.Properties.GetProperties("Property");

        values.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetProperty_ForEmptyProject_ThrowException()
    {
        var content = """
                      <Project>
                        <PropertyGroup>
                        </PropertyGroup>
                      </Project>
                      """;

        var sut = DotnetProjectFile.Create(content);

        Assert.Throws<DotnetProjectSystemException>(() =>
        {
            sut.Properties.GetProperty("TargetFramework");
        });
    }

    [Fact]
    public void GetProperty_ForProjectWithDuplicatedProperties_ThrowException()
    {
        var content = """
                      <Project>
                        <PropertyGroup>
                          <TargetFramework>net8.0</TargetFramework>
                          <TargetFramework>net8.0</TargetFramework>
                        </PropertyGroup>
                      </Project>
                      """;

        var sut = DotnetProjectFile.Create(content);

        Assert.Throws<DotnetProjectSystemException>(() =>
        {
            sut.Properties.GetProperty("TargetFramework");
        });
    }

    [Fact]
    public void GetProperty_ForProjectWithProperty_ReturnValue()
    {
        var content = """
                      <Project>
                        <PropertyGroup>
                          <TargetFramework>net8.0</TargetFramework>
                        </PropertyGroup>
                      </Project>
                      """;

        var sut = DotnetProjectFile.Create(content);

        DotnetProjectProperty compileItems = sut.Properties.GetProperty("TargetFramework");

        compileItems.Should().Be(new DotnetProjectProperty("TargetFramework", "net8.0"));
    }

    [Fact]
    public void AddProperty_ForEmptyProject_ReturnExpectedContent()
    {
        const string expected = """
                                <Project>
                                  <PropertyGroup>
                                    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                                  </PropertyGroup>
                                </Project>
                                """;

        DotnetProjectFile projectFile = DotnetProjectFile.CreateEmpty();
        projectFile
            .Properties
            .AddProperty("ManagePackageVersionsCentrally", "true");

        projectFile.ToXmlString(_formatter).Should().Be(expected);
    }

    [Fact]
    public void AddOrUpdateProperty_ForEmptyProject_ReturnExpectedContent()
    {
        const string expected = """
                                <Project>
                                  <PropertyGroup>
                                    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                                  </PropertyGroup>
                                </Project>
                                """;

        DotnetProjectFile projectFile = DotnetProjectFile.CreateEmpty();
        projectFile
            .Properties
            .SetProperty("ManagePackageVersionsCentrally", "true");

        projectFile.ToXmlString(_formatter).Should().Be(expected);
    }

    [Fact]
    public void AddOrUpdateProperty_ProjectWithSameProperty_ReturnExpectedContent()
    {
        const string expected = """
                                <Project>
                                  <PropertyGroup>
                                    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                                  </PropertyGroup>
                                </Project>
                                """;

        DotnetProjectFile projectFile = DotnetProjectFile.Create(expected);
        projectFile.Properties.SetProperty("ManagePackageVersionsCentrally", "true");

        projectFile.ToXmlString(_formatter).Should().Be(expected);
    }

    [Fact]
    public void AddOrUpdateProperty_ProjectWithDifferentPropertyValue_ReturnExpectedContent()
    {
        const string input = """
                                <Project>
                                  <PropertyGroup>
                                    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
                                  </PropertyGroup>
                                </Project>
                                """;

        const string expected = """
                                <Project>
                                  <PropertyGroup>
                                    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                                  </PropertyGroup>
                                </Project>
                                """;

        DotnetProjectFile projectFile = DotnetProjectFile.Create(input);
        projectFile
            .Properties
            .SetProperty("ManagePackageVersionsCentrally", "true");

        projectFile.ToXmlString(_formatter).Should().Be(expected);
    }

    [Fact]
    public void AddOrUpdateProperty_ProjectWithMultiplePropertyDeclaration_ThrowException()
    {
        const string input = """
                             <Project>
                               <PropertyGroup>
                                 <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
                                 <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
                               </PropertyGroup>
                             </Project>
                             """;

        DotnetProjectFile projectFile = DotnetProjectFile.Create(input);
        Assert.Throws<DotnetProjectSystemException>(() =>
        {
            projectFile.Properties.SetProperty("ManagePackageVersionsCentrally", "true");
        });
    }

    [Fact]
    public void RemoveProperty_ProjectWithProperty_PropertyMustBeRemoved()
    {
        const string input = """
                             <Project>
                               <PropertyGroup>
                                 <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
                               </PropertyGroup>
                             </Project>
                             """;

        const string expected = """
                                <Project>
                                  <PropertyGroup>
                                  </PropertyGroup>
                                </Project>
                                """;

        DotnetProjectFile projectFile = DotnetProjectFile.Create(input);
        projectFile
            .Properties
            .RemoveProperty("ManagePackageVersionsCentrally");

        projectFile.ToXmlString(_formatter).Should().Be(expected);
    }

    [Fact]
    public void GetEnableDefaultItemsOrDefault_ProjectFileSetTrue_ReturnTrue()
    {
        const string input = """
                             <Project>
                               <PropertyGroup>
                                 <EnableDefaultItems>true</EnableDefaultItems>
                               </PropertyGroup>
                             </Project>
                             """;

        DotnetProjectFile projectFile = DotnetProjectFile.Create(input);

        projectFile.Properties.GetEnableDefaultItemsOrDefault().Should().BeTrue();
    }

    [Fact]
    public void GetEnableDefaultItemsOrDefault_ProjectFileSetFalse_ReturnFalse()
    {
        const string input = """
                             <Project>
                               <PropertyGroup>
                                 <EnableDefaultItems>false</EnableDefaultItems>
                               </PropertyGroup>
                             </Project>
                             """;

        DotnetProjectFile projectFile = DotnetProjectFile.Create(input);

        projectFile.Properties.GetEnableDefaultItemsOrDefault().Should().BeFalse();
    }
}