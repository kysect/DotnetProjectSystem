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
}