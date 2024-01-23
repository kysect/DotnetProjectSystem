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
}