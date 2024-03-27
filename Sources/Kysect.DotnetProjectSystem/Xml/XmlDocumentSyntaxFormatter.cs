using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Exceptions;
using Kysect.DotnetProjectSystem.Tools;
using Microsoft.Language.Xml;
using System.Text;

namespace Kysect.DotnetProjectSystem.Xml;

public class XmlDocumentSyntaxFormatter
{
    public const string DefaultIndention = "  ";

    public XmlDocumentSyntax Format(XmlDocumentSyntax documentSyntax)
    {
        documentSyntax.ThrowIfNull();

        List<XmlNodeSyntax> elements = documentSyntax
            .Descendants()
            .OfType<IXmlElement>()
            .Select(x => x.AsSyntaxElement.AsNode)
            .ToList();

        documentSyntax =
            documentSyntax
                .ReplaceNodes(
                    elements,
                    (o, u) => UpdateElement(documentSyntax, (IXmlElement) o, (IXmlElement) u));

        return documentSyntax;
    }

    private XmlNodeSyntax UpdateElement(XmlDocumentSyntax documentSyntax, IXmlElement oldNode, IXmlElement currentElement)
    {
        currentElement.ThrowIfNull();

        int depth = CalculateDepth(documentSyntax, oldNode);
        currentElement = FormatAttributes(currentElement);
        currentElement = AddLeadingTrivia(depth, currentElement);

        return currentElement.AsSyntaxElement.AsNode;
    }

    private int CalculateDepth(XmlDocumentSyntax _, IXmlElement currentElement)
    {
        int depth = 0;

        while (currentElement.Parent is not null)
        {
            if (depth > 10)
                throw new DotnetProjectSystemException("Cannot calculate XML element depth. Possible StackOverflow.");

            depth++;
            currentElement = currentElement.Parent;
        }

        return depth;
    }

    private IXmlElement FormatAttributes(IXmlElement currentElement)
    {
        IXmlElementSyntax currentElementSyntax = currentElement.AsSyntaxElement;

        bool needRemoveTrailingTriviaForNameNode = currentElementSyntax.Attributes.Any();
        if (needRemoveTrailingTriviaForNameNode)
        {
            XmlNameSyntax nameNodeWithoutTrailingTrivia = currentElementSyntax.NameNode.WithoutTrailingTrivia();
            currentElementSyntax = (IXmlElementSyntax) currentElementSyntax.AsNode.ReplaceNode(currentElement.AsSyntaxElement.NameNode, nameNodeWithoutTrailingTrivia);
        }

        if (currentElementSyntax.Attributes.Any())
        {
            List<PunctuationSyntax> punctuationSyntaxes = currentElementSyntax
                .AsNode
                .DescendantNodes()
                .OfType<PunctuationSyntax>()
                .Where(p => p.Kind == SyntaxKind.SlashGreaterThanToken)
                .ToList();

            currentElementSyntax = (IXmlElementSyntax) currentElementSyntax.AsNode.ReplaceNodes(punctuationSyntaxes, (_, n) =>
            {
                return n
                    .WithLeadingTrivia(SyntaxFactory.WhitespaceTrivia(" "))
                    .WithoutTrailingTrivia();
            });
        }

        currentElementSyntax = (IXmlElementSyntax) currentElementSyntax.AsNode.ReplaceNodes(currentElementSyntax.Attributes, (_, n) =>
        {
            return n
                .WithLeadingTrivia(SyntaxFactory.WhitespaceTrivia(" "))
                .WithoutTrailingTrivia();
        });

        return currentElementSyntax.AsElement;
    }

    private IXmlElement AddLeadingTrivia(int depth, IXmlElement modified)
    {
        if (modified is XmlEmptyElementSyntax xmlEmptyElementSyntax)
            return AddLeadingTrivia(depth, xmlEmptyElementSyntax);

        if (modified is XmlElementSyntax xmlElementSyntax)
            return AddLeadingTrivia(depth, xmlElementSyntax);

        throw SwitchDefaultExceptions.OnUnexpectedType(modified);
    }

    private IXmlElement AddLeadingTrivia(int depth, XmlEmptyElementSyntax xmlEmptyElementSyntax)
    {
        string trivia = GetTrivia(depth);

        xmlEmptyElementSyntax = xmlEmptyElementSyntax.ReplaceNode(
            xmlEmptyElementSyntax.LessThanToken,
            xmlEmptyElementSyntax.LessThanToken.WithLeadingTrivia(SyntaxFactory.WhitespaceTrivia(trivia)));

        return xmlEmptyElementSyntax;
    }

    private IXmlElement AddLeadingTrivia(int depth, XmlElementSyntax xmlElementSyntax)
    {
        string trivia = GetTrivia(depth);

        // TODO: remove this hack for first node
        bool needTriviaForStartTag = depth != 0;
        if (needTriviaForStartTag)
        {
            xmlElementSyntax = xmlElementSyntax.ReplaceNode(
                xmlElementSyntax.StartTag,
                xmlElementSyntax.StartTag.WithLeadingTrivia(SyntaxFactory.WhitespaceTrivia(trivia)));
        }

        bool needTriviaForEndTag = xmlElementSyntax.Elements.Any();
        if (needTriviaForEndTag)
        {
            xmlElementSyntax = xmlElementSyntax.ReplaceNode(
                xmlElementSyntax.EndTag,
                xmlElementSyntax.EndTag.WithLeadingTrivia(SyntaxFactory.WhitespaceTrivia(trivia)));

        }

        return xmlElementSyntax;
    }

    private string GetTrivia(int depth)
    {
        var sb = new StringBuilder(Environment.NewLine);

        for (int i = 0; i < depth; i++)
            sb = sb.Append(DefaultIndention);

        return sb.ToString();
    }
}