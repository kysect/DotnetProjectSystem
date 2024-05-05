using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Projects;

public class DotnetProjectFilePackageReferences(DotnetProjectFile projectFile)
{
    public DotnetProjectFile AddPackageReference(string name)
    {
        IXmlElementSyntax itemGroup = projectFile.GetOrAddItemGroup();
        IXmlElementSyntax packageReference = ExtendedSyntaxFactory
            .XmlEmptyElement(DotnetProjectFileConstant.PackageReference)
            .AddAttribute(ExtendedSyntaxFactory.XmlAttribute("Include", name));

        projectFile.AddChildAndUpdateDocument(itemGroup, packageReference);
        return projectFile;
    }

    public DotnetProjectFile AddPackageReference(string name, string version)
    {
        IXmlElementSyntax itemGroup = projectFile.GetOrAddItemGroup();
        IXmlElementSyntax packageReference = ExtendedSyntaxFactory
            .XmlEmptyElement(DotnetProjectFileConstant.PackageReference)
            .AddAttribute(ExtendedSyntaxFactory.XmlAttribute("Include", name))
            .AddAttribute(ExtendedSyntaxFactory.XmlAttribute("Version", version));

        projectFile.AddChildAndUpdateDocument(itemGroup, packageReference);
        return projectFile;
    }

    public IReadOnlyCollection<ProjectPackageReference> GetPackageReferences()
    {
        var result = new List<ProjectPackageReference>();

        foreach (IXmlElementSyntax xmlElementSyntax in projectFile.GetNodesByName(DotnetProjectFileConstant.PackageReference))
        {
            XmlAttributeSyntax nameAttribute = xmlElementSyntax.GetAttribute("Include").ThrowIfNull();
            XmlAttributeSyntax? versionAttribute = xmlElementSyntax.GetAttribute("Version");

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

        foreach (IXmlElementSyntax xmlElementSyntax in projectFile.GetNodesByName(DotnetProjectFileConstant.PackageReference))
        {
            XmlAttributeSyntax nameAttribute = xmlElementSyntax.GetAttribute("Include").ThrowIfNull();

            if (nameAttribute.Value != name)
                continue;

            nodes.Add(xmlElementSyntax.AsNode);
        }

        projectFile.UpdateDocument(d => d.RemoveNodes(nodes, SyntaxRemoveOptions.KeepNoTrivia));
    }

    public void SetPackageReference(string name, string version)
    {
        foreach (IXmlElementSyntax xmlElementSyntax in projectFile.GetNodesByName(DotnetProjectFileConstant.PackageReference))
        {
            XmlAttributeSyntax nameAttribute = xmlElementSyntax.GetAttribute("Include").ThrowIfNull();
            if (nameAttribute.Value != name)
                continue;

            XmlAttributeSyntax versionAttribute = xmlElementSyntax.GetAttribute("Version").ThrowIfNull();
            XmlAttributeSyntax? changedVersionAttribute = versionAttribute.WithValue(ExtendedSyntaxFactory.XmlString(version));
            projectFile.UpdateDocument(d => d.ReplaceNode(versionAttribute, changedVersionAttribute));

            return;
        }
    }

    public void RemoveVersion(string name)
    {
        foreach (IXmlElementSyntax xmlElementSyntax in projectFile.GetNodesByName(DotnetProjectFileConstant.PackageReference))
        {
            XmlAttributeSyntax nameAttribute = xmlElementSyntax.GetAttribute("Include").ThrowIfNull();
            if (nameAttribute.Value != name)
                continue;

            XmlAttributeSyntax versionAttribute = xmlElementSyntax.GetAttribute("Version").ThrowIfNull();
            IXmlElementSyntax changedElement = xmlElementSyntax.RemoveAttribute(versionAttribute);
            projectFile.UpdateDocument(d => d.ReplaceNode(xmlElementSyntax.AsNode, changedElement.AsNode));

            return;
        }
    }
}