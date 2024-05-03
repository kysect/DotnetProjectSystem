using Kysect.DotnetProjectSystem.Projects;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Tests.Projects;

public class DirectoryBuildTargetFileTests
{
    [Fact]
    public void Create_FileWithTarget_ReturnFileWithTarget()
    {
        var content = """
                      <Project>
                        <Target Name="SomeTargetName" BeforeTargets="BeforeBuild">
                          <PropertyGroup Condition="'$(SomeProperty)' == 'true'">
                            <SomeProperty>false</SomeProperty>
                          </PropertyGroup>
                        </Target>
                      </Project>
                      """;

        var dotnetBuildTargetFile = DirectoryBuildTargetFile.Create(content);

        IReadOnlyCollection<IXmlElementSyntax> targets = dotnetBuildTargetFile.GetTargets();

        targets.Should().HaveCount(1);
    }
}