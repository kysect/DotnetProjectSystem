using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Projects;

public class DirectoryPackagesPropsVersions
{
    private readonly DotnetProjectFile _projectFile;

    public DirectoryPackagesPropsVersions(DotnetProjectFile projectFile)
    {
        _projectFile = projectFile;
    }

    public IReadOnlyCollection<ProjectPackageVersion> GetPackageVersions()
    {
        var result = new List<ProjectPackageVersion>();

        foreach (IXmlElementSyntax xmlElementSyntax in _projectFile.GetNodesByName(DotnetProjectFileConstant.PackageVersion))
        {
            XmlAttributeSyntax nameAttribute = xmlElementSyntax.GetAttribute("Include").ThrowIfNull();
            XmlAttributeSyntax versionAttribute = xmlElementSyntax.GetAttribute("Version").ThrowIfNull();

            result.Add(new ProjectPackageVersion(nameAttribute.Value, versionAttribute.Value));
        }

        return result;
    }

    public void AddPackageVersion(string name, string version)
    {
        IXmlElementSyntax itemGroup = _projectFile.GetOrAddItemGroup();
        IXmlElementSyntax packageReference = ExtendedSyntaxFactory
            .XmlEmptyElement(DotnetProjectFileConstant.PackageVersion)
            .AddAttribute(ExtendedSyntaxFactory.XmlAttribute("Include", name))
            .AddAttribute(ExtendedSyntaxFactory.XmlAttribute("Version", version));

        _projectFile.AddChildAndUpdateDocument(itemGroup, packageReference);
    }

    public void RemovePackageVersion(string name)
    {
        var nodes = new List<SyntaxNode>();

        foreach (IXmlElementSyntax xmlElementSyntax in _projectFile.GetNodesByName(DotnetProjectFileConstant.PackageVersion))
        {
            XmlAttributeSyntax nameAttribute = xmlElementSyntax.GetAttribute("Include").ThrowIfNull();
            if (nameAttribute.Value != name)
                continue;

            nodes.Add(xmlElementSyntax.AsNode);
        }

        _projectFile.UpdateDocument(d => d.RemoveNodes(nodes, SyntaxRemoveOptions.KeepNoTrivia));
    }

    public void SetPackageVersion(string name, string version)
    {
        foreach (IXmlElementSyntax xmlElementSyntax in _projectFile.GetNodesByName(DotnetProjectFileConstant.PackageVersion))
        {
            XmlAttributeSyntax nameAttribute = xmlElementSyntax.GetAttribute("Include").ThrowIfNull();
            if (nameAttribute.Value != name)
                continue;

            XmlAttributeSyntax versionAttribute = xmlElementSyntax.GetAttribute("Version").ThrowIfNull();
            XmlAttributeSyntax? changedVersionAttribute = versionAttribute.WithValue(ExtendedSyntaxFactory.XmlString(version));
            _projectFile.UpdateDocument(d => d.ReplaceNode(versionAttribute, changedVersionAttribute));

            return;
        }
    }
}