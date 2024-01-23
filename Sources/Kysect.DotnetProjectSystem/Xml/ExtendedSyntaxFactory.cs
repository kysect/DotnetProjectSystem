using Microsoft.Language.Xml;
using System.Text;

namespace Kysect.DotnetProjectSystem.Xml;

public static class ExtendedSyntaxFactory
{
    public const string DefaultIndention = "  ";

    public static XmlNameSyntax XmlName(string name)
    {
        return SyntaxFactory.XmlName(null, SyntaxFactory.XmlNameToken(name, null, null));
    }

    public static XmlElementSyntax XmlElement(string name, int depth)
    {
        var startTagSyntax = SyntaxFactory.XmlElementStartTag(
            SyntaxFactory.Punctuation(SyntaxKind.LessThanToken, "<", XmlWhiteIndention(depth), null),
            XmlName(name),
            SyntaxFactory.List<XmlAttributeSyntax>(),
            SyntaxFactory.Punctuation(SyntaxKind.GreaterThanToken, ">", SyntaxFactory.WhitespaceTrivia(string.Empty), null)
        );

        var endTagSyntax = SyntaxFactory.XmlElementEndTag(
            SyntaxFactory.Punctuation(SyntaxKind.LessThanSlashToken, "</", XmlWhiteIndention(depth), null),
            XmlName(name),
            SyntaxFactory.Punctuation(SyntaxKind.GreaterThanToken, ">", SyntaxFactory.WhitespaceTrivia(string.Empty), null)
        );

        return SyntaxFactory.XmlElement(startTagSyntax, SyntaxFactory.List<SyntaxNode>(), endTagSyntax);
    }

    public static SyntaxTrivia XmlWhiteIndention(int depth)
    {
        var sb = new StringBuilder(Environment.NewLine);

        for (int i = 0; i < depth; i++)
            sb = sb.Append(DefaultIndention);

        return SyntaxFactory.WhitespaceTrivia(sb.ToString());
    }
}