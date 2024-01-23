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

        return Create(contentTemplate);
    }
    public static DotnetProjectFile Create(string content)
    {
        XmlDocumentSyntax xmlDocumentSyntax = Parser.ParseText(content);
        if (xmlDocumentSyntax.RootSyntax is null)
            return CreateEmpty();

        if (xmlDocumentSyntax.Root.Name != "Project")
            throw new ArgumentException("XML root must be Project");

        return new DotnetProjectFile(xmlDocumentSyntax);
    }

    public string ToXmlString()
    {
        return _content.ToFullString();
    }
}