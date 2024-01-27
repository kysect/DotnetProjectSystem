using Kysect.CommonLib.BaseTypes.Extensions;
using Microsoft.Language.Xml;

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

    public IReadOnlyCollection<ProjectPackageVersion> GetPackageVersions()
    {
        var result = new List<ProjectPackageVersion>();

        foreach (IXmlElementSyntax xmlElementSyntax in File.GetNodesByName(DotnetProjectFileConstant.PackageVersion))
        {
            XmlAttributeSyntax nameAttribute = xmlElementSyntax.GetAttribute("Include").ThrowIfNull();
            XmlAttributeSyntax versionAttribute = xmlElementSyntax.GetAttribute("Version").ThrowIfNull();

            result.Add(new ProjectPackageVersion(nameAttribute.Value, versionAttribute.Value));
        }

        return result;
    }
}