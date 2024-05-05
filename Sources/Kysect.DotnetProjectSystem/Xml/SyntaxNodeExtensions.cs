using Kysect.CommonLib.BaseTypes.Extensions;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Xml;

public static class SyntaxNodeExtensions
{
    public static IReadOnlyCollection<IXmlElementSyntax> GetNodesByName(this SyntaxNode syntaxNode, string name)
    {
        return syntaxNode
            .Descendants()
            .Where(n => n.Name == name)
            .ToList();
    }

    public static bool IsFirstNodeInDocument(this SyntaxNode element)
    {
        element.ThrowIfNull();

        if (element.Parent is null)
            throw new ArgumentException($"Element doesn't contain parent: {element.ToFullString()}");

        if (element.Parent is not XmlDocumentSyntax xmlDocument)
            return false;

        if (xmlDocument.ChildNodes.First().Equals(element))
            return true;

        return false;
    }
}