using Kysect.CommonLib.BaseTypes.Extensions;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Xml;

public static class XmlElementSyntaxExtensions
{
    public static IXmlElementSyntax RemoveAllChild(this IXmlElementSyntax xmlElementSyntax)
    {
        xmlElementSyntax.ThrowIfNull();

        while (xmlElementSyntax.Elements.Any())
            xmlElementSyntax = xmlElementSyntax.RemoveChild(xmlElementSyntax.Elements.First());

        return xmlElementSyntax;
    }
}