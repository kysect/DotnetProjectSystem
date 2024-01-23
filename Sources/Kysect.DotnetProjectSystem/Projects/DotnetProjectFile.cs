using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Projects;

public class DotnetProjectFile
{
    private readonly XmlDocumentSyntax _content;

    public DotnetProjectFile(XmlDocumentSyntax content)
    {
        _content = content;
    }

    public static DotnetProjectFile CreateEmpty()
    {
        string contentTemplate =
            """
            <Project>
            </Project>
            """;

        return new DotnetProjectFile(Parser.ParseText(contentTemplate));
    }

    public string ToXmlString()
    {
        return _content.ToFullString();
    }
}