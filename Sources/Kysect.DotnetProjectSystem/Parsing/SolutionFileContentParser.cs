using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Tools;
using System.Text.RegularExpressions;

namespace Kysect.DotnetProjectSystem.Parsing;

public class SolutionFileContentParser
{
    private record struct PreparsedSolutionElement(Guid ElementTypeId, string Name, string Path, Guid Id);
    public record struct NestedProject(Guid Child, Guid Parent);

    private static readonly Regex ProjectPattern = new Regex(
        @"Project\(\""(?<typeGuid>.*?)\""\)\s+=\s+\""(?<name>.*?)\"",\s+\""(?<path>.*?)\"",\s+\""(?<guid>.*?)\""(?<content>.*?)\bEndProject\b",
        RegexOptions.ExplicitCapture | RegexOptions.Singleline);

    private static readonly Regex NestedPattern = new Regex(@"\s*{(?<child>.*?)}\s*=\s*{(?<parent>.*?)}\s*");

    public IReadOnlyCollection<DotnetProjectFileDescriptor> ParseSolutionFileContent(string solutionContents)
    {
        IReadOnlyCollection<PreparsedSolutionElement> preparsedSolutionElements = ParseElements(solutionContents);
        IReadOnlyCollection<NestedProject> nestedProject = ParseNestedProject(solutionContents);
        Dictionary<Guid, Guid> childToParentMapping = nestedProject.ToDictionary(n => n.Child, n => n.Parent);
        Dictionary<Guid, string> solutionElementNames = preparsedSolutionElements.ToDictionary(p => p.Id, p => p.Name);

        List<DotnetProjectFileDescriptor> result = new List<DotnetProjectFileDescriptor>();

        foreach (PreparsedSolutionElement preparsedSolutionElement in preparsedSolutionElements)
        {
            bool isPathToProject = preparsedSolutionElement.Path.EndsWith("proj") && !preparsedSolutionElement.Path.EndsWith("vdproj");

            if (!isPathToProject)
                continue;

            string solutionStructurePath = preparsedSolutionElement.Name;
            Guid currentElementId = preparsedSolutionElement.Id;
            while (childToParentMapping.TryGetValue(currentElementId, out Guid parentId))
            {
                string parentName = solutionElementNames[parentId];
                currentElementId = parentId;
                solutionStructurePath = $"{parentName}{Path.DirectorySeparatorChar}{solutionStructurePath}";
            }

            var project = new DotnetProjectFileDescriptor(
                preparsedSolutionElement.ElementTypeId,
                preparsedSolutionElement.Name,
                preparsedSolutionElement.Path,
                solutionStructurePath,
                preparsedSolutionElement.Id);

            result.Add(project);
        }

        return result;
    }

    public IReadOnlyCollection<NestedProject> ParseNestedProject(string solutionContents)
    {
        solutionContents.ThrowIfNull();

        string nestedProjectHeader = "GlobalSection(NestedProjects) = preSolution";
        if (!solutionContents.Contains(nestedProjectHeader))
            return [];

        List<string> parts = solutionContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

        int headerIndex = parts.FindIndex(s => s.Contains(nestedProjectHeader));

        List<NestedProject> result = [];
        int currentIndex = headerIndex + 1;
        while (currentIndex < parts.Count && !parts[currentIndex].Contains("EndGlobalSection"))
        {
            Match match = NestedPattern.Match(parts[currentIndex]);
            if (!match.Success)
                throw new DotnetProjectSystemException($"Cannot extract nested project information from {parts[currentIndex]}");

            string child = match.Groups["child"].Value;
            string parent = match.Groups["parent"].Value;

            result.Add(new NestedProject(Guid.Parse(child), Guid.Parse(parent)));
            currentIndex++;
        }

        return result;
    }

    private IReadOnlyCollection<PreparsedSolutionElement> ParseElements(string solutionContents)
    {
        List<PreparsedSolutionElement> result = [];

        Match match = ProjectPattern.Match(solutionContents);

        while (match.Success)
        {
            string projectTypeIdString = match.Groups["typeGuid"].Value;
            string projectName = match.Groups["name"].Value;
            string projectPath = match.Groups["path"].Value;
            string projectIdString = match.Groups["guid"].Value;

            if (!Guid.TryParse(projectIdString, out Guid projectId))
                throw new DotnetProjectSystemException($"Project id {projectTypeIdString} is not valid id");

            if (!Guid.TryParse(projectTypeIdString, out Guid projectTypeId))
                throw new DotnetProjectSystemException($"Project type id {projectTypeIdString} is not valid id");

            projectPath = FormatPath(projectPath);
            var project = new PreparsedSolutionElement(projectTypeId, projectName, projectPath, projectId);
            result.Add(project);

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