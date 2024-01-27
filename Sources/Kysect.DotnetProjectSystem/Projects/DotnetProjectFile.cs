using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Projects;

public class DotnetProjectFile
{
    private XmlDocumentSyntax _content;

    public DotnetProjectFile(XmlDocumentSyntax content)
    {
        content.ThrowIfNull();
        content.RootSyntax.ThrowIfNull();

        if (content.Root.Name != "Project")
            throw new ArgumentException("XML root must be Project");

        _content = content;
    }

    public static DotnetProjectFile CreateEmpty()
    {
        string contentTemplate =
            """
            <Project>
            </Project>
            """;

        return Create(contentTemplate);
    }

    public static DotnetProjectFile CreateLegacyFormat()
    {
        string contentTemplate =
            """
            <Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
            </Project>
            """;

        return Create(contentTemplate);
    }

    public static DotnetProjectFile Create(string content)
    {
        XmlDocumentSyntax xmlDocumentSyntax = Parser.ParseText(content);
        if (xmlDocumentSyntax.RootSyntax is null)
            return CreateEmpty();

        return new DotnetProjectFile(xmlDocumentSyntax);
    }

    public void UpdateDocument(Func<XmlDocumentSyntax, XmlDocumentSyntax> morphism)
    {
        morphism.ThrowIfNull();
        _content = morphism(_content);
    }

    public void UpdateDocument<TSyntax>(IXmlProjectFileModifyStrategy<TSyntax> modifyStrategy)
        where TSyntax : SyntaxNode
    {
        modifyStrategy.ThrowIfNull();

        IReadOnlyCollection<TSyntax> nodes = modifyStrategy.SelectNodeForModify(_content);
        _content = _content.ReplaceNodes(nodes, (_, n) => modifyStrategy.ApplyChanges(n));
    }

    public IXmlElement GetProjectNode()
    {
        return _content.Root;
    }

    public IReadOnlyCollection<IXmlElementSyntax> GetNodesByName(string name)
    {
        return _content.GetNodesByName(name);
    }

    public IXmlElementSyntax GetOrAddPropertyGroup()
    {
        return GetOrAddProjectChild(DotnetProjectFileConstant.PropertyGroup);
    }

    public IXmlElementSyntax GetOrAddItemGroup()
    {
        return GetOrAddProjectChild(DotnetProjectFileConstant.ItemGroup);
    }

    public IXmlElementSyntax GetOrAddProjectChild(string elementName)
    {
        elementName.ThrowIfNull();

        IXmlElement projectNode = GetProjectNode();
        IXmlElementSyntax? propertyGroupNode = projectNode
            .AsSyntaxElement
            .Descendants()
            .FirstOrDefault(s => s.Name == elementName);

        // TODO: reduce duplication
        if (propertyGroupNode is null)
        {
            propertyGroupNode = ExtendedSyntaxFactory.XmlElement(elementName);
            IXmlElementSyntax changedProjectNode = projectNode
                .AsSyntaxElement
                .AddChild(propertyGroupNode);

            _content = _content.ReplaceNode(projectNode.AsSyntaxElement.AsNode, changedProjectNode.AsNode);
            projectNode = GetProjectNode();
            propertyGroupNode = projectNode
                .AsSyntaxElement
                .Descendants()
                .First(s => s.Name == elementName);
        }

        return propertyGroupNode;
    }

    public bool IsSdkFormat()
    {
        IXmlElement projectNode = GetProjectNode();
        return projectNode.Attributes.All(a => a.Key != DotnetProjectFileConstant.ToolsVersion);
    }

    public IReadOnlyCollection<DotnetProjectItem> GetItems(string group)
    {
        List<DotnetProjectItem> items = _content
            .GetNodesByName(group)
            .Select(n => n.GetAttributeValue("Include"))
            .Where(n => n is not null)
            .Select(n => new DotnetProjectItem(group, n))
            .ToList();

        return items;
    }

    public IReadOnlyCollection<ProjectPackageReference> GetPackageReferences()
    {
        var result = new List<ProjectPackageReference>();

        foreach (IXmlElementSyntax xmlElementSyntax in _content.GetNodesByName(DotnetProjectFileConstant.PackageReference))
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

    public IReadOnlyCollection<DotnetProjectProperty> GetProperties(string property)
    {
        return _content
            .GetNodesByName(property)
            .Select(xmlElementSyntax => new DotnetProjectProperty(property, xmlElementSyntax.Content.ToFullString()))
            .ToList();
    }

    public DotnetProjectProperty? FindProperty(string property)
    {
        IReadOnlyCollection<DotnetProjectProperty> properties = GetProperties(property);

        if (properties.Count > 1)
            throw new DotnetProjectSystemException($"Duplicated property {property}");

        if (properties.Count == 0)
            return null;

        return properties.Single();
    }

    public DotnetProjectProperty GetProperty(string property)
    {
        DotnetProjectProperty? dotnetProjectProperty = FindProperty(property);

        if (dotnetProjectProperty is null)
            throw new DotnetProjectSystemException($"Property {property} missed");

        return dotnetProjectProperty.Value;
    }

    public DotnetProjectFile AddCompileItem(string value)
    {
        return AddItem("Compile", value);
    }

    public DotnetProjectFile AddItem(string type, string value)
    {
        type.ThrowIfNull();
        value.ThrowIfNull();

        IXmlElementSyntax itemSyntax = ExtendedSyntaxFactory
            .XmlEmptyElement(type)
            .AddAttribute(ExtendedSyntaxFactory.XmlAttribute("Include", value));

        IXmlElementSyntax itemGroup = GetOrAddItemGroup();
        IXmlElementSyntax modifiedItemGroup = itemGroup.AddChild(itemSyntax);

        _content = _content.ReplaceNode(itemGroup.AsNode, modifiedItemGroup.AsNode);
        return this;
    }

    public DotnetProjectFile AddOrUpdateProperty(string name, string value)
    {
        DotnetProjectProperty? property = FindProperty(name);
        if (property is null)
            return AddProperty(name, value);

        IReadOnlyCollection<IXmlElementSyntax> properties = _content.GetNodesByName(name);
        if (properties.Count > 1)
            throw new DotnetProjectSystemException("Cannot update property. File contains multiple declarations");

        if (properties.Count == 0)
            throw new DotnetProjectSystemException($"Cannot find property {name} in file.");

        IXmlElementSyntax elementSyntax = properties.Single();
        IXmlElementSyntax changedElement = elementSyntax.WithContent(ExtendedSyntaxFactory.XmlPropertyContent(value));

        _content = _content.ReplaceNode(elementSyntax.AsNode, changedElement.AsNode);
        return this;
    }

    public DotnetProjectFile AddProperty(string name, string value)
    {
        XmlElementSyntax propertyElement = ExtendedSyntaxFactory
            .XmlEmptyElement(name)
            .WithContent(ExtendedSyntaxFactory.XmlPropertyContent(value));

        IXmlElementSyntax propertyGroup = GetOrAddPropertyGroup();
        IXmlElementSyntax modifiedItemGroup = propertyGroup.AddChild(propertyElement);

        _content = _content.ReplaceNode(propertyGroup.AsNode, modifiedItemGroup.AsNode);
        return this;
    }

    public string ToXmlString(XmlDocumentSyntaxFormatter formatter)
    {
        formatter.ThrowIfNull();

        return formatter.Format(_content).ToFullString();
    }
}