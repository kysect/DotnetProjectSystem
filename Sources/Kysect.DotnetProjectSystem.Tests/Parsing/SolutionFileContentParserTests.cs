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

    [Fact]
    public void ParseSolutionFileContent_SolutionWithVdproj_ShouldSkipVdproj()
    {

        var solutionFile = """
                           Microsoft Visual Studio Solution File, Format Version 12.00
                           # Visual Studio Version 17
                           VisualStudioVersion = 17.9.34310.174
                           MinimumVisualStudioVersion = 10.0.40219.1
                           Project("{00000000-0000-0000-0000-000000000000}") = "Kysect.DotnetSlnParser", "Kysect.DotnetSlnParser\Kysect.DotnetSlnParser.csproj", "{00000000-0000-0000-0000-000000000000}"
                           EndProject
                           Project("{00000000-0000-0000-0000-000000000000}") = "Kysect.DotnetSlnParser.Tests", "Kysect.DotnetSlnParser.Tests\Kysect.DotnetSlnParser.Tests.vdproj", "{00000000-0000-0000-0000-000000000000}"
                           EndProject
                           Global
                             GlobalSection(SolutionProperties) = preSolution
                               HideSolutionNode = FALSE
                             EndGlobalSection
                           EndGlobal
                           """;

        List<DotnetProjectFileDescriptor> expected = new List<DotnetProjectFileDescriptor>
        {
            new DotnetProjectFileDescriptor(
                Guid.Empty,
                "Kysect.DotnetSlnParser",
                _fileSystem.Path.Combine("Kysect.DotnetSlnParser", "Kysect.DotnetSlnParser.csproj"),
                Guid.Empty),
        };

        IReadOnlyCollection<DotnetProjectFileDescriptor> projects = _parser.ParseSolutionFileContent(solutionFile);

        projects.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ParseSolutionFileContent_SolutionWithNonProjectFile_ShouldSkipNonProject()
    {
        var solutionFile = """
                           Microsoft Visual Studio Solution File, Format Version 12.00
                           # Visual Studio Version 17
                           VisualStudioVersion = 17.9.34310.174
                           MinimumVisualStudioVersion = 10.0.40219.1
                           Project("{00000000-0000-0000-0000-000000000000}") = "Kysect.DotnetSlnParser", "Kysect.DotnetSlnParser\Kysect.DotnetSlnParser.csproj", "{00000000-0000-0000-0000-000000000000}"
                           EndProject
                           Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Solution Items", "Solution Items", "{BCA4E9AC-020E-4040-90EF-56DA53FD874E}"
                             ProjectSection(SolutionItems) = preProject
                               Directory.Build.props = Directory.Build.props
                             EndProjectSection
                           EndProject
                           Global
                           Global
                             GlobalSection(SolutionProperties) = preSolution
                               HideSolutionNode = FALSE
                             EndGlobalSection
                           EndGlobal
                           """;

        List<DotnetProjectFileDescriptor> expected = new List<DotnetProjectFileDescriptor>
        {
            new DotnetProjectFileDescriptor(
                Guid.Empty,
                "Kysect.DotnetSlnParser",
                _fileSystem.Path.Combine("Kysect.DotnetSlnParser", "Kysect.DotnetSlnParser.csproj"),
                Guid.Empty),
        };

        IReadOnlyCollection<DotnetProjectFileDescriptor> projects = _parser.ParseSolutionFileContent(solutionFile);

        projects.Should().BeEquivalentTo(expected);
    }
}