using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Exceptions;
using Microsoft.Language.Xml;
using System.Text;

namespace Kysect.DotnetProjectSystem.Xml;

public class XmlDocumentSyntaxFormatter
{
    public const string DefaultIndention = "  ";

    public XmlDocumentSyntax Format(XmlDocumentSyntax documentSyntax)
    {
        documentSyntax.ThrowIfNull();

        IXmlElement updatedElement = UpdateElement(documentSyntax.Root, 0);
        documentSyntax = documentSyntax.ReplaceNode(
            documentSyntax.Root.AsSyntaxElement.AsNode,
            updatedElement.AsSyntaxElement.AsNode);

        return documentSyntax;
    }

    private IXmlElement UpdateElement(IXmlElement currentElement, int depth)
    {
        currentElement.ThrowIfNull();

        IXmlElementSyntax modified = currentElement.AsSyntaxElement;
        modified = modified.RemoveAllChild();

        foreach (IXmlElement? element in currentElement.Elements)
        {
            IXmlElement updatedElement = UpdateElement(element, depth + 1);

            modified = modified
                .AddChild(updatedElement.AsSyntaxElement);
        }

        return AddLeadingTrivia(depth, modified);
    }

    private IXmlElement AddLeadingTrivia(int depth, IXmlElementSyntax modified)
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
        xmlElementSyntax.StartTag.WithLeadingTrivia(SyntaxFactory.WhitespaceTrivia(trivia));

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