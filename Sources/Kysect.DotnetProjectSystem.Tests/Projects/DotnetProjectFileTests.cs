using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Xml;

namespace Kysect.DotnetProjectSystem.Tests.Projects;

public class DotnetProjectFileTests
{
    private readonly XmlDocumentSyntaxFormatter _formatter = new XmlDocumentSyntaxFormatter();

    [Fact]
    public void CreateEmpty_ReturnXmlWithProjectNode()
    {
        string expected = """
                          <Project>
                          </Project>
                          """;

        var sut = DotnetProjectFile.CreateEmpty();

        string actual = sut.ToXmlString(_formatter);

        actual.Should().Be(expected);
    }

    [Fact]
    public void Create_FromStringWithProjectNode_ReturnXmlWithProjectNode()
    {
        string expected = """
                          <Project>
                          </Project>
                          """;

        var sut = DotnetProjectFile.Create(expected);

        string actual = sut.ToXmlString(_formatter);

        actual.Should().Be(expected);
    }

    [Fact]
    public void Create_FromEmptyString_ReturnXmlWithProjectNode()
    {
        string expected = """
                          <Project>
                          </Project>
                          """;

        var sut = DotnetProjectFile.Create(string.Empty);

        string actual = sut.ToXmlString(_formatter);

        actual.Should().Be(expected);
    }

    [Fact]
    public void Create_FromStringWithXmlTag_ThrowExceptionAboutInvalidTag()
    {
        var argumentException = Assert.Throws<ArgumentException>(() =>
        {
            DotnetProjectFile.Create("<xml/>");
        });

        argumentException.Should().NotBeNull();
    }

    [Fact]
    public void GetProjectNode_ForEmptyNode_ReturnProjectNode()
    {
        var sut = DotnetProjectFile.CreateEmpty();

        var projectNode = sut.GetProjectNode();

        projectNode.Name.Should().Be("Project");
    }

    [Fact]
    public void GetOrAddPropertyGroup_ForEmptyString_ReturnStringWithPropertyGroup()
    {
        string expected = """
                          <Project>
                            <PropertyGroup>
                            </PropertyGroup>
                          </Project>
                          """;
        var sut = DotnetProjectFile.CreateEmpty();

        var propertyGroupNode = sut.GetOrAddPropertyGroup();
        var actual = sut.ToXmlString(_formatter);

        actual.Should().Be(expected);
    }

    [Fact]
    public void GetOrAddPropertyGroup_ForXmlWithPropertyGroup_DocumentMustNotBeChanged()
    {
        string expected = """
                          <Project>
                            <PropertyGroup>
                            </PropertyGroup>
                          </Project>
                          """;
        var sut = DotnetProjectFile.Create(expected);

        var propertyGroupNode = sut.GetOrAddPropertyGroup();
        var actual = sut.ToXmlString(_formatter);

        actual.Should().Be(expected);
    }

    [Fact]
    public void IsSdkFormat_ForEmptyFile_ReturnTrue()
    {
        var sut = DotnetProjectFile.CreateEmpty();

        sut.IsSdkFormat().Should().BeTrue();
    }

    [Fact]
    public void IsSdkFormat_ForLegacyFormat_ReturnFalse()
    {
        var sut = DotnetProjectFile.Create(@"<Project ToolsVersion=""15.0""></Project>");

        sut.IsSdkFormat().Should().BeFalse();
    }

    [Fact]
    public void GetItems_ForProjectWithCompileNode_ReturnItem()
    {
        var content = """
                      <Project>
                        <ItemGroup>
                          <Compile Include="File.cs">
                          <Compile Include="File2.cs">
                        </ItemGroup>
                      </Project>
                      """;
        DotnetProjectItem[] exptected = [
            new DotnetProjectItem("Compile", "File.cs"),
            new DotnetProjectItem("Compile", "File2.cs")
        ];

        var sut = DotnetProjectFile.Create(content);

        IReadOnlyCollection<DotnetProjectItem> compileItems = sut.GetItems("Compile");

        compileItems.Should().BeEquivalentTo(exptected);
    }
}