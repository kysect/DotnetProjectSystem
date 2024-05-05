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

        return documentSyntax.ReplaceElements<IXmlElement>(FormatElement);
    }

    private XmlNodeSyntax FormatElement(IXmlElement oldNode, IXmlElement updatedNode)
    {
        updatedNode.ThrowIfNull();

        int indentCount = oldNode.GetNodeDepthIndex();
        int originalNewLineCount = CalculateOriginalNewLineCount(updatedNode);

        updatedNode = FormatAttributes(updatedNode);
        updatedNode = AddLeadingTrivia(originalNewLineCount, indentCount, oldNode, updatedNode);

        return updatedNode.AsSyntaxElement.AsNode;
    }

    private int CalculateOriginalNewLineCount(IXmlElement currentElement)
    {
        int newLineCount = currentElement
            .AsSyntaxElement
            .AsNode
            .GetLeadingTrivia()
            .ToFullString()
            .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
            .Length - 1;

        return newLineCount;
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

    private IXmlElement AddLeadingTrivia(int originalNewLineCount, int indentCount, IXmlElement oldNode, IXmlElement modified)
    {
        bool isFirstNodeInDocument = oldNode.AsSyntaxElement.AsNode.IsFirstNodeInDocument();

        if (modified is XmlEmptyElementSyntax xmlEmptyElementSyntax)
            return AddLeadingTrivia(originalNewLineCount, indentCount, xmlEmptyElementSyntax, isFirstNodeInDocument);

        if (modified is XmlElementSyntax xmlElementSyntax)
            return AddLeadingTrivia(originalNewLineCount, indentCount, xmlElementSyntax, isFirstNodeInDocument);

        throw SwitchDefaultExceptions.OnUnexpectedType(modified);
    }

    private IXmlElement AddLeadingTrivia(int originalNewLineCount, int indentCount, XmlEmptyElementSyntax xmlEmptyElementSyntax, bool isFirstNodeInDocument)
    {
        int newLineCount = isFirstNodeInDocument ? 0 : Math.Max(originalNewLineCount, 1);
        string trivia = GetTrivia(indentCount, newLineCount);

        xmlEmptyElementSyntax = xmlEmptyElementSyntax.ReplaceNode(
            xmlEmptyElementSyntax.LessThanToken,
            xmlEmptyElementSyntax.LessThanToken.WithLeadingTrivia(SyntaxFactory.WhitespaceTrivia(trivia)));

        return xmlEmptyElementSyntax;
    }

    private IXmlElement AddLeadingTrivia(int originalNewLineCount, int indentCount, XmlElementSyntax xmlElementSyntax, bool isFirstNodeInDocument)
    {
        int openNodeNewLineCount = isFirstNodeInDocument ? 0 : Math.Max(originalNewLineCount, 1);
        string openNodeTrivia = GetTrivia(indentCount, openNodeNewLineCount);
        xmlElementSyntax = xmlElementSyntax.ReplaceNode(
                xmlElementSyntax.StartTag,
                xmlElementSyntax.StartTag.WithLeadingTrivia(SyntaxFactory.WhitespaceTrivia(openNodeTrivia)));

        bool needTriviaForEndTag = xmlElementSyntax.Elements.Any();
        int closeNodeNewLineCount = needTriviaForEndTag ? 1 : 0;
        string closeNodeTrivia = GetTrivia(indentCount, closeNodeNewLineCount);
        if (needTriviaForEndTag)
        {
            xmlElementSyntax = xmlElementSyntax.ReplaceNode(
                xmlElementSyntax.EndTag,
                xmlElementSyntax.EndTag.WithLeadingTrivia(SyntaxFactory.WhitespaceTrivia(closeNodeTrivia)));
        }

        return xmlElementSyntax;
    }

    private string GetTrivia(int depth, int newLineCount)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < newLineCount; i++)
            sb = sb.AppendLine();

        for (int i = 0; i < depth; i++)
            sb = sb.Append(DefaultIndention);

        return sb.ToString();
    }
}