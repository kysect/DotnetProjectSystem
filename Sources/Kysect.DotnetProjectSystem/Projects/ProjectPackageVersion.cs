using Kysect.CommonLib.BaseTypes.Extensions;

namespace Kysect.DotnetProjectSystem.Projects;

public class ProjectPackageVersion
{
    public string Name { get; }
    public string Version { get; }

    public ProjectPackageVersion(string name, string version)
    {
        Name = name.ThrowIfNull();
        Version = version.ThrowIfNull();
    }
}