using Kysect.DotnetProjectSystem.Projects;

namespace Kysect.DotnetProjectSystem.Parsing;

public record DotnetSolutionDescriptor(string FilePath, Dictionary<string, DotnetProjectFile> Projects);