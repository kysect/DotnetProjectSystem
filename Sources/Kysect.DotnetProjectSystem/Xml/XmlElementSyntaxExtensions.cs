using Kysect.CommonLib.BaseTypes.Extensions;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Xml;

public static class XmlElementSyntaxExtensions
{
    public static IXmlElementSyntax RemoveAllChild(this IXmlElementSyntax xmlElementSyntax)
    {
        xmlElementSyntax.ThrowIfNull();

        IXmlElementSyntax? child = xmlElementSyntax.Elements.FirstOrDefault();

        while (child is not null)
        {
            xmlElementSyntax = xmlElementSyntax.RemoveChild(child);
            child = xmlElementSyntax.Elements.FirstOrDefault();
        }

        return xmlElementSyntax;
    }
}