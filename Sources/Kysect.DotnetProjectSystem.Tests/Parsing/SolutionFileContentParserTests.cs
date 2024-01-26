using Kysect.DotnetProjectSystem.FileStructureBuilding;
using Kysect.DotnetProjectSystem.Parsing;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.Parsing;

public class SolutionFileContentParserTests
{
    private readonly SolutionFileContentParser _parser;
    private readonly MockFileSystem _fileSystem;

    public SolutionFileContentParserTests()
    {
        _parser = new SolutionFileContentParser();
        _fileSystem = new MockFileSystem();
    }

    [Fact]
    public void ParseSolutionFileContent_ThisSolution_ReturnExpectedResult()
    {
        var solutionBuilder = new SolutionFileStructureBuilder("Solution")
            .AddProject(new ProjectFileStructureBuilder("Kysect.DotnetSlnParser"))
            .AddProject(new ProjectFileStructureBuilder("Kysect.DotnetSlnParser.Tests"));

        string solutionFile = solutionBuilder.CreateSolutionFile(_fileSystem);

        List<DotnetProjectFileDescriptor> expected = new List<DotnetProjectFileDescriptor>
        {
            new DotnetProjectFileDescriptor(
                Guid.Empty,
                "Kysect.DotnetSlnParser",
                _fileSystem.Path.Combine("Kysect.DotnetSlnParser", "Kysect.DotnetSlnParser.csproj"),
                Guid.Empty),

            new DotnetProjectFileDescriptor(
                Guid.Empty,
                "Kysect.DotnetSlnParser.Tests",
                _fileSystem.Path.Combine("Kysect.DotnetSlnParser.Tests", "Kysect.DotnetSlnParser.Tests.csproj"),
                Guid.Empty),
        };

        IReadOnlyCollection<DotnetProjectFileDescriptor> projects = _parser.ParseSolutionFileContent(solutionFile);

        projects.Should().BeEquivalentTo(expected);
    }
}