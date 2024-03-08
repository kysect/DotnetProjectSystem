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
        var solutionFile = """
                           Microsoft Visual Studio Solution File, Format Version 12.00
                           # Visual Studio Version 17
                           VisualStudioVersion = 17.9.34310.174
                           MinimumVisualStudioVersion = 10.0.40219.1
                           Project("{00000000-0000-0000-0000-000000000000}") = "Kysect.DotnetSlnParser", "Kysect.DotnetSlnParser\Kysect.DotnetSlnParser.csproj", "{5F22CD28-BC0A-4A18-BD9C-32561220ADE2}"
                           EndProject
                           Project("{00000000-0000-0000-0000-000000000000}") = "Kysect.DotnetSlnParser.Tests", "Kysect.DotnetSlnParser.Tests\Kysect.DotnetSlnParser.Tests.csproj", "{F878A59D-187D-48BC-8A50-F602E6F324A1}"
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
                "Kysect.DotnetSlnParser",
                Guid.Parse("5F22CD28-BC0A-4A18-BD9C-32561220ADE2")),

            new DotnetProjectFileDescriptor(
                Guid.Empty,
                "Kysect.DotnetSlnParser.Tests",
                _fileSystem.Path.Combine("Kysect.DotnetSlnParser.Tests", "Kysect.DotnetSlnParser.Tests.csproj"),
                "Kysect.DotnetSlnParser.Tests",
                Guid.Parse("F878A59D-187D-48BC-8A50-F602E6F324A1")),
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
                           Project("{00000000-0000-0000-0000-000000000000}") = "Kysect.DotnetSlnParser", "Kysect.DotnetSlnParser\Kysect.DotnetSlnParser.csproj", "{5F22CD28-BC0A-4A18-BD9C-32561220ADE2}"
                           EndProject
                           Project("{00000000-0000-0000-0000-000000000000}") = "Kysect.DotnetSlnParser.Tests", "Kysect.DotnetSlnParser.Tests\Kysect.DotnetSlnParser.Tests.vdproj", "{F878A59D-187D-48BC-8A50-F602E6F324A1}"
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
                "Kysect.DotnetSlnParser",
                Guid.Parse("5F22CD28-BC0A-4A18-BD9C-32561220ADE2")),
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
                "Kysect.DotnetSlnParser",
                Guid.Empty),
        };

        IReadOnlyCollection<DotnetProjectFileDescriptor> projects = _parser.ParseSolutionFileContent(solutionFile);

        projects.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ParseSolutionFileContent_SolutionWithDirectories_ReturnCorrectDirectoryStructure()
    {
        var solutionFileContent = """
                                  Microsoft Visual Studio Solution File, Format Version 12.00
                                  # Visual Studio Version 17
                                  VisualStudioVersion = 17.10.34607.79
                                  MinimumVisualStudioVersion = 10.0.40219.1
                                  Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "SolutionWithDirectories", "SolutionWithDirectories\SolutionWithDirectories.csproj", "{53CAC67D-6BD9-43B6-B531-B63302742E4E}"
                                  EndProject
                                  Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Dir1", "Dir1", "{A8180B9A-AD9E-4826-92C0-8FBED3DE76E5}"
                                  EndProject
                                  Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Dir2", "Dir2", "{D6D58295-33E7-49D1-B8FE-B4DA509070EF}"
                                  EndProject
                                  Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Dir11", "Dir11", "{E64851B0-4B11-4502-B85F-86BBC6A3E22C}"
                                  EndProject
                                  Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Project1", "Project1\Project1.csproj", "{ADAF4B0E-EB3F-4568-A1AF-5568D2718BB5}"
                                  EndProject
                                  Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Project11", "Project11\Project11.csproj", "{44859C40-86C9-4CC9-B798-7F2922F307F9}"
                                  EndProject
                                  Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Project2", "Project2\Project2.csproj", "{7AE5C6BD-E859-4663-AA3A-13BFB42573AF}"
                                  EndProject
                                  Global
                                  	GlobalSection(SolutionConfigurationPlatforms) = preSolution
                                  		Debug|Any CPU = Debug|Any CPU
                                  		Release|Any CPU = Release|Any CPU
                                  	EndGlobalSection
                                  	GlobalSection(ProjectConfigurationPlatforms) = postSolution
                                  		{53CAC67D-6BD9-43B6-B531-B63302742E4E}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                                  		{53CAC67D-6BD9-43B6-B531-B63302742E4E}.Debug|Any CPU.Build.0 = Debug|Any CPU
                                  		{53CAC67D-6BD9-43B6-B531-B63302742E4E}.Release|Any CPU.ActiveCfg = Release|Any CPU
                                  		{53CAC67D-6BD9-43B6-B531-B63302742E4E}.Release|Any CPU.Build.0 = Release|Any CPU
                                  		{ADAF4B0E-EB3F-4568-A1AF-5568D2718BB5}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                                  		{ADAF4B0E-EB3F-4568-A1AF-5568D2718BB5}.Debug|Any CPU.Build.0 = Debug|Any CPU
                                  		{ADAF4B0E-EB3F-4568-A1AF-5568D2718BB5}.Release|Any CPU.ActiveCfg = Release|Any CPU
                                  		{ADAF4B0E-EB3F-4568-A1AF-5568D2718BB5}.Release|Any CPU.Build.0 = Release|Any CPU
                                  		{44859C40-86C9-4CC9-B798-7F2922F307F9}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                                  		{44859C40-86C9-4CC9-B798-7F2922F307F9}.Debug|Any CPU.Build.0 = Debug|Any CPU
                                  		{44859C40-86C9-4CC9-B798-7F2922F307F9}.Release|Any CPU.ActiveCfg = Release|Any CPU
                                  		{44859C40-86C9-4CC9-B798-7F2922F307F9}.Release|Any CPU.Build.0 = Release|Any CPU
                                  		{7AE5C6BD-E859-4663-AA3A-13BFB42573AF}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                                  		{7AE5C6BD-E859-4663-AA3A-13BFB42573AF}.Debug|Any CPU.Build.0 = Debug|Any CPU
                                  		{7AE5C6BD-E859-4663-AA3A-13BFB42573AF}.Release|Any CPU.ActiveCfg = Release|Any CPU
                                  		{7AE5C6BD-E859-4663-AA3A-13BFB42573AF}.Release|Any CPU.Build.0 = Release|Any CPU
                                  	EndGlobalSection
                                  	GlobalSection(SolutionProperties) = preSolution
                                  		HideSolutionNode = FALSE
                                  	EndGlobalSection
                                  	GlobalSection(NestedProjects) = preSolution
                                  		{E64851B0-4B11-4502-B85F-86BBC6A3E22C} = {A8180B9A-AD9E-4826-92C0-8FBED3DE76E5}
                                  		{ADAF4B0E-EB3F-4568-A1AF-5568D2718BB5} = {A8180B9A-AD9E-4826-92C0-8FBED3DE76E5}
                                  		{44859C40-86C9-4CC9-B798-7F2922F307F9} = {E64851B0-4B11-4502-B85F-86BBC6A3E22C}
                                  		{7AE5C6BD-E859-4663-AA3A-13BFB42573AF} = {D6D58295-33E7-49D1-B8FE-B4DA509070EF}
                                  	EndGlobalSection
                                  	GlobalSection(ExtensibilityGlobals) = postSolution
                                  		SolutionGuid = {D8697CA6-8793-4142-B7E8-43F27312707D}
                                  	EndGlobalSection
                                  EndGlobal
                                  """;

        List<DotnetProjectFileDescriptor> expected =
        [
            new DotnetProjectFileDescriptor(
                Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"),
                "SolutionWithDirectories",
                _fileSystem.Path.Combine("SolutionWithDirectories", "SolutionWithDirectories.csproj"),
                "SolutionWithDirectories",
                Guid.Parse("53CAC67D-6BD9-43B6-B531-B63302742E4E")),

            new DotnetProjectFileDescriptor(
                Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"),
                "Project1",
                _fileSystem.Path.Combine("Project1", "Project1.csproj"),
                _fileSystem.Path.Combine("Dir1", "Project1"),
                Guid.Parse("ADAF4B0E-EB3F-4568-A1AF-5568D2718BB5")),

            new DotnetProjectFileDescriptor(
                Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"),
                "Project11",
                _fileSystem.Path.Combine("Project11", "Project11.csproj"),
                _fileSystem.Path.Combine("Dir1", "Dir11", "Project11"),
                Guid.Parse("44859C40-86C9-4CC9-B798-7F2922F307F9")),

            new DotnetProjectFileDescriptor(
                Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"),
                "Project2",
                _fileSystem.Path.Combine("Project2", "Project2.csproj"),
                _fileSystem.Path.Combine("Dir2", "Project2"),
                Guid.Parse("7AE5C6BD-E859-4663-AA3A-13BFB42573AF"))

        ];

        IReadOnlyCollection<DotnetProjectFileDescriptor> projects = _parser.ParseSolutionFileContent(solutionFileContent);

        projects.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ParseNestedProject_SolutionWithDirectories_ReturnDirectories()
    {
        var solutionFileContent = """
                                  Microsoft Visual Studio Solution File, Format Version 12.00
                                  # Visual Studio Version 17
                                  VisualStudioVersion = 17.10.34607.79
                                  MinimumVisualStudioVersion = 10.0.40219.1
                                  Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "SolutionWithDirectories", "SolutionWithDirectories\SolutionWithDirectories.csproj", "{53CAC67D-6BD9-43B6-B531-B63302742E4E}"
                                  EndProject
                                  Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Dir1", "Dir1", "{A8180B9A-AD9E-4826-92C0-8FBED3DE76E5}"
                                  EndProject
                                  Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Dir2", "Dir2", "{D6D58295-33E7-49D1-B8FE-B4DA509070EF}"
                                  EndProject
                                  Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Dir11", "Dir11", "{E64851B0-4B11-4502-B85F-86BBC6A3E22C}"
                                  EndProject
                                  Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Project1", "Project1\Project1.csproj", "{ADAF4B0E-EB3F-4568-A1AF-5568D2718BB5}"
                                  EndProject
                                  Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Project11", "Project11\Project11.csproj", "{44859C40-86C9-4CC9-B798-7F2922F307F9}"
                                  EndProject
                                  Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Project2", "Project2\Project2.csproj", "{7AE5C6BD-E859-4663-AA3A-13BFB42573AF}"
                                  EndProject
                                  Global
                                  	GlobalSection(SolutionConfigurationPlatforms) = preSolution
                                  		Debug|Any CPU = Debug|Any CPU
                                  		Release|Any CPU = Release|Any CPU
                                  	EndGlobalSection
                                  	GlobalSection(ProjectConfigurationPlatforms) = postSolution
                                  		{53CAC67D-6BD9-43B6-B531-B63302742E4E}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                                  		{53CAC67D-6BD9-43B6-B531-B63302742E4E}.Debug|Any CPU.Build.0 = Debug|Any CPU
                                  		{53CAC67D-6BD9-43B6-B531-B63302742E4E}.Release|Any CPU.ActiveCfg = Release|Any CPU
                                  		{53CAC67D-6BD9-43B6-B531-B63302742E4E}.Release|Any CPU.Build.0 = Release|Any CPU
                                  		{ADAF4B0E-EB3F-4568-A1AF-5568D2718BB5}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                                  		{ADAF4B0E-EB3F-4568-A1AF-5568D2718BB5}.Debug|Any CPU.Build.0 = Debug|Any CPU
                                  		{ADAF4B0E-EB3F-4568-A1AF-5568D2718BB5}.Release|Any CPU.ActiveCfg = Release|Any CPU
                                  		{ADAF4B0E-EB3F-4568-A1AF-5568D2718BB5}.Release|Any CPU.Build.0 = Release|Any CPU
                                  		{44859C40-86C9-4CC9-B798-7F2922F307F9}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                                  		{44859C40-86C9-4CC9-B798-7F2922F307F9}.Debug|Any CPU.Build.0 = Debug|Any CPU
                                  		{44859C40-86C9-4CC9-B798-7F2922F307F9}.Release|Any CPU.ActiveCfg = Release|Any CPU
                                  		{44859C40-86C9-4CC9-B798-7F2922F307F9}.Release|Any CPU.Build.0 = Release|Any CPU
                                  		{7AE5C6BD-E859-4663-AA3A-13BFB42573AF}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                                  		{7AE5C6BD-E859-4663-AA3A-13BFB42573AF}.Debug|Any CPU.Build.0 = Debug|Any CPU
                                  		{7AE5C6BD-E859-4663-AA3A-13BFB42573AF}.Release|Any CPU.ActiveCfg = Release|Any CPU
                                  		{7AE5C6BD-E859-4663-AA3A-13BFB42573AF}.Release|Any CPU.Build.0 = Release|Any CPU
                                  	EndGlobalSection
                                  	GlobalSection(SolutionProperties) = preSolution
                                  		HideSolutionNode = FALSE
                                  	EndGlobalSection
                                  	GlobalSection(NestedProjects) = preSolution
                                  		{E64851B0-4B11-4502-B85F-86BBC6A3E22C} = {A8180B9A-AD9E-4826-92C0-8FBED3DE76E5}
                                  		{ADAF4B0E-EB3F-4568-A1AF-5568D2718BB5} = {A8180B9A-AD9E-4826-92C0-8FBED3DE76E5}
                                  		{44859C40-86C9-4CC9-B798-7F2922F307F9} = {E64851B0-4B11-4502-B85F-86BBC6A3E22C}
                                  		{7AE5C6BD-E859-4663-AA3A-13BFB42573AF} = {D6D58295-33E7-49D1-B8FE-B4DA509070EF}
                                  	EndGlobalSection
                                  	GlobalSection(ExtensibilityGlobals) = postSolution
                                  		SolutionGuid = {D8697CA6-8793-4142-B7E8-43F27312707D}
                                  	EndGlobalSection
                                  EndGlobal
                                  """;

        var expected = new List<SolutionFileContentParser.NestedProject>
        {
            new SolutionFileContentParser.NestedProject(
                Guid.Parse("E64851B0-4B11-4502-B85F-86BBC6A3E22C"),
                Guid.Parse("A8180B9A-AD9E-4826-92C0-8FBED3DE76E5")),

            new SolutionFileContentParser.NestedProject(
                Guid.Parse("ADAF4B0E-EB3F-4568-A1AF-5568D2718BB5"),
                Guid.Parse("A8180B9A-AD9E-4826-92C0-8FBED3DE76E5")),

            new SolutionFileContentParser.NestedProject(
                Guid.Parse("44859C40-86C9-4CC9-B798-7F2922F307F9"),
                Guid.Parse("E64851B0-4B11-4502-B85F-86BBC6A3E22C")),

            new SolutionFileContentParser.NestedProject(
                Guid.Parse("7AE5C6BD-E859-4663-AA3A-13BFB42573AF"),
                Guid.Parse("D6D58295-33E7-49D1-B8FE-B4DA509070EF"))
        };

        IReadOnlyCollection<SolutionFileContentParser.NestedProject> nestedProjects = _parser.ParseNestedProject(solutionFileContent);

        nestedProjects.Should().BeEquivalentTo(expected);
    }
}