using Kysect.DotnetProjectSystem.Projects;

namespace Kysect.DotnetProjectSystem.Tests.Projects;

public class DirectoryBuildPropsFileTests
{
    [Fact]
    public void ArtifactsOutputEnabled_ForEmptyFile_ReturnFalse()
    {
        const string input = """
                             <Project>
                             </Project>
                             """;

        var directoryPackagesPropsFile = new DirectoryBuildPropsFile(DotnetProjectFile.Create(input));

        bool actual = directoryPackagesPropsFile.ArtifactsOutputEnabled();

        actual.Should().Be(false);
    }

    [Fact]
    public void ArtifactsOutputEnabled_ForEnabled_ReturnTrue()
    {
        const string input = """
                             <Project>
                               <PropertyGroup>
                                 <UseArtifactsOutput>true</UseArtifactsOutput>
                               </PropertyGroup>
                             </Project>
                             """;

        var directoryPackagesPropsFile = new DirectoryBuildPropsFile(DotnetProjectFile.Create(input));

        bool actual = directoryPackagesPropsFile.ArtifactsOutputEnabled();

        actual.Should().Be(true);
    }

    [Fact]
    public void ArtifactsOutputEnabled_ForDisabled_ReturnTrue()
    {
        const string input = """
                             <Project>
                               <PropertyGroup>
                                 <UseArtifactsOutput>false</UseArtifactsOutput>
                               </PropertyGroup>
                             </Project>
                             """;

        var directoryPackagesPropsFile = new DirectoryBuildPropsFile(DotnetProjectFile.Create(input));

        bool actual = directoryPackagesPropsFile.ArtifactsOutputEnabled();

        actual.Should().Be(false);
    }
}