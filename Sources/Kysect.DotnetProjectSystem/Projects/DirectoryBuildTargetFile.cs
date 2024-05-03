using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Projects;

public class DirectoryBuildTargetFile
{
    private XmlDocumentSyntax _content;

    public static DirectoryBuildTargetFile CreateEmpty()
    {
        string contentTemplate =
            """
            <Project>
            </Project>
            """;

        return Create(contentTemplate);
    }

    public static DirectoryBuildTargetFile Create(string content)
    {
        XmlDocumentSyntax xmlDocumentSyntax = Parser.ParseText(content);
        if (xmlDocumentSyntax.RootSyntax is null)
            return CreateEmpty();

        return new DirectoryBuildTargetFile(xmlDocumentSyntax);
    }

    public DirectoryBuildTargetFile(XmlDocumentSyntax content)
    {
        _content = content;
    }

    public IReadOnlyCollection<IXmlElementSyntax> GetTargets()
    {
        return GetNodesByName("Target");
    }

    public IReadOnlyCollection<IXmlElementSyntax> GetNodesByName(string name)
    {
        return _content.GetNodesByName(name);
    }

    public void UpdateDocument(Func<XmlDocumentSyntax, XmlDocumentSyntax> morphism)
    {
        morphism.ThrowIfNull();
        _content = morphism(_content);
    }

    public string ToXmlString(XmlDocumentSyntaxFormatter formatter)
    {
        formatter.ThrowIfNull();

        return formatter.Format(_content).ToFullString();
    }
}