namespace Kysect.DotnetProjectSystem.Parsing;

public record DotnetProjectFileDescriptor(
    Guid ProjectTypeGuid,
    string ProjectName,
    string FileSystemPath,
    string SolutionStructurePath,
    Guid ProjectGuid);