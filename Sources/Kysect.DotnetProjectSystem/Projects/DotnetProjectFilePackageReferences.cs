using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Projects;

public class DotnetProjectFilePackageReferences
{
    private readonly DotnetProjectFile _projectFile;

    public DotnetProjectFilePackageReferences(DotnetProjectFile projectFile)
    {
        _projectFile = projectFile;
    }

    public DotnetProjectFilePackageReferences AddPackageReference(string name)
    {
        IXmlElementSyntax itemGroup = _projectFile.GetOrAddItemGroup();
        IXmlElementSyntax packageReference = ExtendedSyntaxFactory
            .XmlEmptyElement(DotnetProjectFileConstant.PackageReference)
            .AddAttribute(ExtendedSyntaxFactory.XmlAttribute("Include", name));

        _projectFile.AddChildAndUpdateDocument(itemGroup, packageReference);
        return this;
    }

    public DotnetProjectFilePackageReferences AddPackageReference(string name, string version)
    {
        IXmlElementSyntax itemGroup = _projectFile.GetOrAddItemGroup();
        IXmlElementSyntax packageReference = ExtendedSyntaxFactory
            .XmlEmptyElement(DotnetProjectFileConstant.PackageReference)
            .AddAttribute(ExtendedSyntaxFactory.XmlAttribute("Include", name))
            .AddAttribute(ExtendedSyntaxFactory.XmlAttribute("Version", version));

        _projectFile.AddChildAndUpdateDocument(itemGroup, packageReference);
        return this;
    }

    public IReadOnlyCollection<ProjectPackageReference> GetPackageReferences()
    {
        var result = new List<ProjectPackageReference>();

        foreach (IXmlElementSyntax xmlElementSyntax in _projectFile.GetNodesByName(DotnetProjectFileConstant.PackageReference))
        {
            XmlAttributeSyntax? nameAttribute = xmlElementSyntax.GetAttribute("Include");
            XmlAttributeSyntax? versionAttribute = xmlElementSyntax.GetAttribute("Version");

            if (nameAttribute is null)
                continue;

            if (versionAttribute is null)
                result.Add(new ProjectPackageReference(nameAttribute.Value, Version: null));
            else
                result.Add(new ProjectPackageReference(nameAttribute.Value, versionAttribute.Value));
        }

        return result;
    }

    public void RemovePackageReference(string name)
    {
        var nodes = new List<SyntaxNode>();

        foreach (IXmlElementSyntax xmlElementSyntax in _projectFile.GetNodesByName(DotnetProjectFileConstant.PackageReference))
        {
            XmlAttributeSyntax? nameAttribute = xmlElementSyntax.GetAttribute("Include");
            if (nameAttribute is null)
                continue;

            if (nameAttribute.Value != name)
                continue;

            nodes.Add(xmlElementSyntax.AsNode);
        }

        _projectFile.UpdateDocument(d => d.RemoveNodes(nodes, SyntaxRemoveOptions.KeepNoTrivia));
    }

    public void UpdatePackageReference(string name, string? version)
    {
        foreach (IXmlElementSyntax xmlElementSyntax in _projectFile.GetNodesByName(DotnetProjectFileConstant.PackageReference))
        {
            XmlAttributeSyntax? nameAttribute = xmlElementSyntax.GetAttribute("Include");
            if (nameAttribute is null)
                continue;

            if (nameAttribute.Value != name)
                continue;

            XmlAttributeSyntax versionAttribute = xmlElementSyntax.GetAttribute("Version").ThrowIfNull();

            if (version is null)
            {
                IXmlElementSyntax changedElement = xmlElementSyntax.RemoveAttribute(versionAttribute);
                _projectFile.UpdateDocument(d => d.ReplaceNode(xmlElementSyntax.AsNode, changedElement.AsNode));
            }
            else
            {
                XmlAttributeSyntax? changedVersionAttribute = versionAttribute.WithValue(ExtendedSyntaxFactory.XmlString(version));
                _projectFile.UpdateDocument(d => d.ReplaceNode(versionAttribute, changedVersionAttribute));
            }

            return;
        }
    }
}