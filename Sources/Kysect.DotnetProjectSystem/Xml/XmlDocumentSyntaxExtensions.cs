using Kysect.CommonLib.BaseTypes.Extensions;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Xml;

public static class XmlDocumentSyntaxExtensions
{
    public static XmlDocumentSyntax ReplaceElements<T>(this XmlDocumentSyntax document, Func<T, T, XmlNodeSyntax> morphism) where T : IXmlElement
    {
        document.ThrowIfNull();

        List<XmlNodeSyntax> elements = document
            .Descendants()
            .OfType<T>()
            .Select(x => x.AsSyntaxElement.AsNode)
            .ToList();

        return document.ReplaceNodes(elements, (o, u) => morphism(o.To<T>(), u.To<T>()));
    }
}