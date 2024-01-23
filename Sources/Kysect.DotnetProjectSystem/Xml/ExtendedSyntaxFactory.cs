using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Xml;

public static class ExtendedSyntaxFactory
{
    public static XmlNameSyntax XmlName(string name)
    {
        return SyntaxFactory.XmlName(null, SyntaxFactory.XmlNameToken(name, null, null));
    }

    public static XmlElementSyntax XmlElement(string name)
    {
        var startTagSyntax = SyntaxFactory.XmlElementStartTag(
            SyntaxFactory.Punctuation(SyntaxKind.LessThanToken, "<", null, null),
            XmlName(name),
            SyntaxFactory.List<XmlAttributeSyntax>(),
            SyntaxFactory.Punctuation(SyntaxKind.GreaterThanToken, ">", SyntaxFactory.WhitespaceTrivia(string.Empty), null)
        );

        var endTagSyntax = SyntaxFactory.XmlElementEndTag(
            SyntaxFactory.Punctuation(SyntaxKind.LessThanSlashToken, "</", null, null),
            XmlName(name),
            SyntaxFactory.Punctuation(SyntaxKind.GreaterThanToken, ">", SyntaxFactory.WhitespaceTrivia(string.Empty), null)
        );

        return SyntaxFactory.XmlElement(startTagSyntax, SyntaxFactory.List<SyntaxNode>(), endTagSyntax);
    }
}