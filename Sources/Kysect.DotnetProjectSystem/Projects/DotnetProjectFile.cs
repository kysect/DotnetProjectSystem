using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Projects;

public class DotnetProjectFile
{
    private XmlDocumentSyntax _content;

    public DotnetProjectFilePackageReferences PackageReferences { get; }
    public DotnetProjectFileProperties Properties { get; }

    public DotnetProjectFile(XmlDocumentSyntax content)
    {
        content.ThrowIfNull();
        content.RootSyntax.ThrowIfNull();

        if (content.Root.Name != "Project")
            throw new ArgumentException("XML root must be Project");

        _content = content;
        PackageReferences = new DotnetProjectFilePackageReferences(this);
        Properties = new DotnetProjectFileProperties(this);
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
        // TODO: add API for returning more than one group
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

    public string ToXmlString(XmlDocumentSyntaxFormatter formatter)
    {
        formatter.ThrowIfNull();

        return formatter.Format(_content).ToFullString();
    }

    internal void AddChildAndUpdateDocument(IXmlElementSyntax parent, IXmlElementSyntax newChild)
    {
        IXmlElementSyntax modifiedParent = parent.AddChild(newChild);
        _content = _content.ReplaceNode(parent.AsNode, modifiedParent.AsNode);
    }
}