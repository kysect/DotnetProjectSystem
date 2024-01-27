namespace Kysect.DotnetProjectSystem.Projects;

public class DirectoryPackagesPropsFile
{
    public DotnetProjectFile File { get; }

    public DirectoryPackagesPropsFile(DotnetProjectFile file)
    {
        File = file;
    }

    public void SetCentralPackageManagement(bool enabled)
    {
        File.AddProperty("ManagePackageVersionsCentrally", enabled.ToString());
    }
}