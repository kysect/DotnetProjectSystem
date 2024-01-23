using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.Parsing;

public class SolutionFileContentParserTests
{
    private readonly SolutionFileContentParser _parser;

    public SolutionFileContentParserTests()
    {
        _parser = new SolutionFileContentParser();
    }

    [Fact]
    public void ParseSolutionFileContent_ThisSolution_ReturnExpectedResult()
    {
        var solutionBuilder = new SolutionFileStructureBuilder("Solution")
            .AddProject(new ProjectFileStructureBuilder("Kysect.DotnetSlnParser", string.Empty))
            .AddProject(new ProjectFileStructureBuilder("Kysect.DotnetSlnParser.Tests", string.Empty));

        var fileSystem = new MockFileSystem();
        string solutionFile = solutionBuilder.CreateSolutionFile(fileSystem);

        List<DotnetProjectFileDescriptor> expected = new List<DotnetProjectFileDescriptor>
        {
            new DotnetProjectFileDescriptor(
                Guid.Empty,
                "Kysect.DotnetSlnParser",
                fileSystem.Path.Combine("Kysect.DotnetSlnParser", "Kysect.DotnetSlnParser.csproj"),
                Guid.Empty),

            new DotnetProjectFileDescriptor(
                Guid.Empty,
                "Kysect.DotnetSlnParser.Tests",
                fileSystem.Path.Combine("Kysect.DotnetSlnParser.Tests", "Kysect.DotnetSlnParser.Tests.csproj"),
                Guid.Empty),
        };

        List<DotnetProjectFileDescriptor> projects = _parser.ParseSolutionFileContent(solutionFile).ToList();

        projects.Should().BeEquivalentTo(expected);
    }
}