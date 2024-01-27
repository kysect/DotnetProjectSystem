namespace Kysect.DotnetProjectSystem.Projects;

public record struct DotnetProjectItem(string Group, string Include);
public record struct ProjectPackageReferences(string Name, string? Version);