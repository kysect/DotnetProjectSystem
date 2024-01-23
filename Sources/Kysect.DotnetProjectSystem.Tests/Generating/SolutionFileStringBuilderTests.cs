using Kysect.DotnetProjectSystem.Generating;

namespace Kysect.DotnetProjectSystem.Tests.Generating;

public class SolutionFileStringBuilderTests
{
    private readonly SolutionFileStringBuilder _sut;

    public SolutionFileStringBuilderTests()
    {
        _sut = new SolutionFileStringBuilder();
    }

    [Fact]
    public void Build_WithoutModification_ReturnTemplateString()
    {
        var expected = """
                       Microsoft Visual Studio Solution File, Format Version 12.00
                       # Visual Studio Version 17
                       VisualStudioVersion = 17.9.34310.174
                       MinimumVisualStudioVersion = 10.0.40219.1
                       Global
                         GlobalSection(SolutionProperties) = preSolution
                           HideSolutionNode = FALSE
                         EndGlobalSection
                       EndGlobal
                       """;

        string slnFileContent = _sut.Build();

        slnFileContent.Should().Be(expected);
    }

    [Fact]
    public void Build_AddProject_AddStringWithStringName()
    {
        string projectName = "Project";
        string projectPath = @"Project\Project.csproj";

        string slnFileContent = _sut
            .AddProject(projectName, projectPath)
            .Build();

        slnFileContent.Should().Contain($"= \"{projectName}\", \"{projectPath}\",");
    }
}