using Kysect.DotnetProjectSystem.Projects;

namespace Kysect.DotnetProjectSystem.Tests.Projects;

public class DotnetProjectFileTests
{
    [Fact]
    public void CreateEmpty_ReturnXmlWithProjectNode()
    {
        string expected = """
                          <Project>
                          </Project>
                          """;

        var sut = DotnetProjectFile.CreateEmpty();

        string actual = sut.ToXmlString();

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

        string actual = sut.ToXmlString();

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

        string actual = sut.ToXmlString();

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
        var actual = sut.ToXmlString();

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
        var actual = sut.ToXmlString();

        actual.Should().Be(expected);
    }
}