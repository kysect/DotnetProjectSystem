using Kysect.DotnetProjectSystem.Projects;

namespace Kysect.DotnetProjectSystem.SolutionModification;

public class DotnetProjectModifier
{
    public DotnetProjectFile File { get; }

    public DotnetProjectModifier(DotnetProjectFile projectFile)
    {
        File = projectFile;
    }

    public bool SupportModification()
    {
        return File.IsSdkFormat();
    }
}