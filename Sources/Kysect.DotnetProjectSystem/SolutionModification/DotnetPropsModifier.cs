using Kysect.DotnetProjectSystem.Projects;

namespace Kysect.DotnetProjectSystem.SolutionModification;

public class DotnetPropsModifier
{
    public DotnetProjectFile File { get; }

    public DotnetPropsModifier(DotnetProjectFile projectFile)
    {
        File = projectFile;
    }
}