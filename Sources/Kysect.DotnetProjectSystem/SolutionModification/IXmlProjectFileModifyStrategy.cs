using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.SolutionModification;

public interface IXmlProjectFileModifyStrategy<TSyntax>
    where TSyntax : SyntaxNode
{
    IReadOnlyCollection<TSyntax> SelectNodeForModify(XmlDocumentSyntax document);
    SyntaxNode ApplyChanges(TSyntax syntax);
}