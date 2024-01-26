using System.Text;

namespace Kysect.DotnetProjectSystem.FileStructureBuilding;

public class SolutionFileStringBuilder
{
    private readonly StringBuilder _builder;

    public SolutionFileStringBuilder()
    {
        const string header = """
                              Microsoft Visual Studio Solution File, Format Version 12.00
                              # Visual Studio Version 17
                              VisualStudioVersion = 17.9.34310.174
                              MinimumVisualStudioVersion = 10.0.40219.1
                              """;

        _builder = new StringBuilder();
        _builder.AppendLine(header);
    }

    public string Build()
    {
        const string footer = """
                              Global
                                GlobalSection(SolutionProperties) = preSolution
                                  HideSolutionNode = FALSE
                                EndGlobalSection
                              EndGlobal
                              """;

        _builder.Append(footer);
        return _builder.ToString();
    }

    public SolutionFileStringBuilder AddProject(string projectName, string projectPath)
    {
        // TODO: Support generating of unique GUID
        string projectDefinition = $$"""
                                     Project("{{{Guid.Empty}}}") = "{{projectName}}", "{{projectPath}}", "{{{Guid.Empty}}}"
                                     EndProject
                                     """;

        _builder.AppendLine(projectDefinition);

        return this;
    }
}