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
}