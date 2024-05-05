using Kysect.CommonLib.BaseTypes.Extensions;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Xml;

public static class XmlElementExtensions
{
    public static int GetNodeDepthIndex(this IXmlElement element)
    {
        element.ThrowIfNull();

        int depth = 0;

        while (element.Parent is not null)
        {
            depth++;
            element = element.Parent;
        }

        return depth;
    }
}