using Kysect.DotnetProjectSystem.Tools;

namespace Kysect.DotnetProjectSystem.Projects;

public class DirectoryPackagesPropsFile
{
    public DotnetProjectFile File { get; }
    public DirectoryPackagesPropsVersions Versions { get; }

    public DirectoryPackagesPropsFile(DotnetProjectFile file)
    {
        File = file;
        Versions = new DirectoryPackagesPropsVersions(file);
    }

    public bool GetCentralPackageManagement()
    {
        return File.Properties.FindBooleanProperty(DotnetProjectFileConstant.ManagePackageVersionsCentrally) ?? false;
    }

    public void SetCentralPackageManagement(bool enabled)
    {
        File.Properties.SetProperty(DotnetProjectFileConstant.ManagePackageVersionsCentrally, enabled);
    }
}