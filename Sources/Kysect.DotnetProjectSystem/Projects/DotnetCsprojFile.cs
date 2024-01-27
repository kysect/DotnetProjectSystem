namespace Kysect.DotnetProjectSystem.Projects;

public class DotnetCsprojFile
{
    public DotnetProjectFile File { get; }

    public DotnetCsprojFile(DotnetProjectFile projectFile)
    {
        File = projectFile;
    }
}