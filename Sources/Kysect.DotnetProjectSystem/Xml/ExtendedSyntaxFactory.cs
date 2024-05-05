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

    public static XmlEmptyElementSyntax XmlEmptyElement(string name)
    {
        return SyntaxFactory.XmlEmptyElement(
            SyntaxFactory.Punctuation(SyntaxKind.LessThanToken, "<", null, null),
            XmlName(name),
            SyntaxFactory.List<XmlAttributeSyntax>(),
            SyntaxFactory.Punctuation(SyntaxKind.SlashGreaterThanToken, "/>", SyntaxFactory.WhitespaceTrivia(" "), null));
    }

    public static XmlAttributeSyntax XmlAttribute(string key, string value)
    {
        return SyntaxFactory.XmlAttribute(
            SyntaxFactory.XmlName(null, SyntaxFactory.XmlNameToken(key, SyntaxFactory.WhitespaceTrivia(" "), null)),
            SyntaxFactory.Punctuation(SyntaxKind.EqualsToken, "=", null, null),
            XmlString(value));
    }

    public static XmlStringSyntax XmlString(string name)
    {
        return SyntaxFactory.XmlString(
            SyntaxFactory.Punctuation(SyntaxKind.DoubleQuoteToken, "\"", null, null),
            XmlName(name),
            SyntaxFactory.Punctuation(SyntaxKind.DoubleQuoteToken, "\"", null, null));
    }

    public static SyntaxList<SyntaxNode> XmlPropertyContent(string value)
    {
        return SyntaxFactory.List<SyntaxNode>(SyntaxFactory.XmlText(SyntaxFactory.XmlTextLiteralToken(value, null, null)));
    }
}