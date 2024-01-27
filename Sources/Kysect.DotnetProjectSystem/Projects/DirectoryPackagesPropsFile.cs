using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Projects;

public class DirectoryPackagesPropsFile
{
    public DotnetProjectFile File { get; }

    public DirectoryPackagesPropsFile(DotnetProjectFile file)
    {
        File = file;
    }

    public bool GetCentralPackageManagement()
    {
        return File.Properties.FindBooleanProperty(DotnetProjectFileConstant.ManagePackageVersionsCentrally) ?? false;
    }

    public void SetCentralPackageManagement(bool enabled)
    {
        File.Properties.SetProperty(DotnetProjectFileConstant.ManagePackageVersionsCentrally, enabled);
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

    public void AddPackageVersion(string name, string version)
    {
        IXmlElementSyntax itemGroup = File.GetOrAddItemGroup();
        IXmlElementSyntax packageReference = ExtendedSyntaxFactory
            .XmlEmptyElement(DotnetProjectFileConstant.PackageVersion)
            .AddAttribute(ExtendedSyntaxFactory.XmlAttribute("Include", name))
            .AddAttribute(ExtendedSyntaxFactory.XmlAttribute("Version", version));

        File.AddChildAndUpdateDocument(itemGroup, packageReference);
    }
}