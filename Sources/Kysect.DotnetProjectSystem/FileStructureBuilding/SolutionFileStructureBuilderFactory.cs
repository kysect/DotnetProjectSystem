using Kysect.DotnetProjectSystem.Xml;
using System.IO.Abstractions;

namespace Kysect.DotnetProjectSystem.FileStructureBuilding;

public class SolutionFileStructureBuilderFactory(IFileSystem fileSystem, XmlDocumentSyntaxFormatter syntaxFormatter)
{
    public SolutionFileStructureBuilder Create(string solutionName)
    {
        return new SolutionFileStructureBuilder(fileSystem, syntaxFormatter, solutionName);
    }
}