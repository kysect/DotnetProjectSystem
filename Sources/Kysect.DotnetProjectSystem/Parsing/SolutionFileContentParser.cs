using Kysect.DotnetProjectSystem.Tools;
using System.Text.RegularExpressions;

namespace Kysect.DotnetProjectSystem.Parsing;

public class SolutionFileContentParser
{
    private static readonly Regex ProjectPattern = new Regex(
        @"Project\(\""(?<typeGuid>.*?)\""\)\s+=\s+\""(?<name>.*?)\"",\s+\""(?<path>.*?)\"",\s+\""(?<guid>.*?)\""(?<content>.*?)\bEndProject\b",
        RegexOptions.ExplicitCapture | RegexOptions.Singleline);

    public IReadOnlyCollection<DotnetProjectFileDescriptor> ParseSolutionFileContent(string solutionContents)
    {
        List<DotnetProjectFileDescriptor> result = new List<DotnetProjectFileDescriptor>();

        Match match = ProjectPattern.Match(solutionContents);

        while (match.Success)
        {
            string projectName = match.Groups["name"].Value;
            string projectPath = match.Groups["path"].Value;
            string projectIdString = match.Groups["guid"].Value;
            string projectTypeIdString = match.Groups["typeGuid"].Value;

            bool isPathToProject = projectPath.EndsWith("proj") && !projectPath.EndsWith("vdproj");

            if (isPathToProject)
            {
                if (!Guid.TryParse(projectIdString, out Guid projectId))
                    throw new DotnetProjectSystemException($"Project id {projectTypeIdString} is not valid id");

                if (!Guid.TryParse(projectTypeIdString, out Guid projectTypeId))
                    throw new DotnetProjectSystemException($"Project type id {projectTypeIdString} is not valid id");

                projectPath = FormatPath(projectPath);
                var project = new DotnetProjectFileDescriptor(projectTypeId, projectName, projectPath, projectId);
                result.Add(project);
            }

            match = match.NextMatch();
        }

        return result;
    }

    private string FormatPath(string path)
    {
        return path
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
    }
}