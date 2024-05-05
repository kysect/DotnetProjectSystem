using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Projects;

public class DotnetProjectFileProperties(DotnetProjectFile projectFile)
{
    public bool? FindEnableDefaultItems()
    {
        return FindBooleanProperty(DotnetProjectFileConstant.EnableDefaultItems);
    }

    public bool GetEnableDefaultItemsOrDefault()
    {
        return FindEnableDefaultItems() ?? projectFile.IsSdkFormat();
    }

    public bool? FindBooleanProperty(string propertyName)
    {
        propertyName.ThrowIfNull();

        DotnetProjectProperty? property = FindProperty(propertyName);
        if (property is null)
            return null;

        if (!bool.TryParse(property.Value.Value, out bool result))
            throw new DotnetProjectSystemException($"Cannot parse project property value {property} to bool");

        return result;
    }

    public IReadOnlyCollection<DotnetProjectProperty> GetProperties(string property)
    {
        return projectFile
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

    public DotnetProjectFileProperties SetProperty(string name, bool value)
    {
        return SetProperty(name, value.ToString().ToLower());
    }

    public DotnetProjectFileProperties SetProperty(string name, string value)
    {
        IReadOnlyCollection<IXmlElementSyntax> properties = projectFile.GetNodesByName(name);
        if (properties.Count == 0)
            return AddProperty(name, value);

        if (properties.Count > 1)
            throw new DotnetProjectSystemException("Cannot update property. File contains multiple declarations");

        IXmlElementSyntax elementSyntax = properties.Single();
        IXmlElementSyntax changedElement = elementSyntax.WithContent(ExtendedSyntaxFactory.XmlPropertyContent(value));

        projectFile.UpdateDocument(d => d.ReplaceNode(elementSyntax.AsNode, changedElement.AsNode));
        return this;
    }

    public DotnetProjectFileProperties AddProperty(string name, string value)
    {
        IXmlElementSyntax propertyGroup = projectFile.GetOrAddPropertyGroup();
        XmlElementSyntax propertyElement = ExtendedSyntaxFactory
            .XmlEmptyElement(name)
            .WithContent(ExtendedSyntaxFactory.XmlPropertyContent(value));

        projectFile.AddChildAndUpdateDocument(propertyGroup, propertyElement);
        return this;
    }

    public DotnetProjectFileProperties RemoveProperty(string name)
    {
        IReadOnlyCollection<IXmlElementSyntax> properties = projectFile.GetNodesByName(name);

        if (properties.Count == 0)
            return this;

        List<XmlNodeSyntax> nodes = properties
            .Select(p => p.AsNode)
            .ToList();

        projectFile.UpdateDocument(d => d.RemoveNodes(nodes, SyntaxRemoveOptions.KeepNoTrivia));
        return this;
    }
}