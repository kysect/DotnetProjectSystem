using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.SolutionModification;

public class SetTargetFrameworkModifyStrategy(string value) : IXmlProjectFileModifyStrategy<XmlElementSyntax>
{
    public IReadOnlyCollection<XmlElementSyntax> SelectNodeForModify(XmlDocumentSyntax document)
    {
        document.ThrowIfNull();

        return document
            .GetNodesByName("TargetFramework")
            .OfType<XmlElementSyntax>()
            .ToList();
    }

    public SyntaxNode ApplyChanges(XmlElementSyntax syntax)
    {
        syntax.ThrowIfNull();

        XmlTextSyntax content = SyntaxFactory.XmlText(SyntaxFactory.XmlTextLiteralToken(value, null, null));
        return syntax.ReplaceNode(syntax.Content.Single(), content);
    }
}