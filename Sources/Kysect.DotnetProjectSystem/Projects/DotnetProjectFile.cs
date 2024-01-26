using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Language.Xml;
using System.IO.Abstractions;
using System.Xml.Linq;

namespace Kysect.DotnetProjectSystem.Projects;

public class DotnetProjectFile
{
    private XmlDocumentSyntax _content;

    public static DotnetProjectFile Create(string path, IFileSystem fileSystem, ILogger logger)
    {
        path.ThrowIfNull();
        fileSystem.ThrowIfNull();
        logger.ThrowIfNull();

        string csprojContent =
            fileSystem.File.Exists(path)
                ? fileSystem.File.ReadAllText(path)
                : string.Empty;

        XmlDocumentSyntax root = Parser.ParseText(csprojContent);
        return new DotnetProjectFile(root);
    }

    public DotnetProjectFile(XmlDocumentSyntax content)
    {
        content.ThrowIfNull();

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

    public IXmlElementSyntax GetOrAddPropertyGroup()
    {
        IXmlElement projectNode = GetProjectNode();
        IXmlElementSyntax? propertyGroupNode = projectNode
            .AsSyntaxElement
            .Descendants()
            .FirstOrDefault(s => s.Name == DotnetProjectFileConstant.PropertyGroup);

        if (propertyGroupNode is not null)
            return propertyGroupNode;

        IXmlElementSyntax changedProjectNode = projectNode
            .AsSyntaxElement
            .AddChild(ExtendedSyntaxFactory.XmlElement(DotnetProjectFileConstant.PropertyGroup));

        _content = _content.ReplaceNode(projectNode.AsSyntaxElement.AsNode, changedProjectNode.AsNode);
        return changedProjectNode;
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

    public string ToXmlString(XmlDocumentSyntaxFormatter formatter)
    {
        formatter.ThrowIfNull();

        return formatter.Format(_content).ToFullString();
    }
}