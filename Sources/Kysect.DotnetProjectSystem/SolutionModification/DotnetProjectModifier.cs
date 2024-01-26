using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.DotnetProjectSystem.SolutionModification;

public class DotnetProjectModifier
{
    public string Path { get; }
    public DotnetProjectFile Accessor { get; }

    private readonly IFileSystem _fileSystem;

    public DotnetProjectModifier(string path, IFileSystem fileSystem, ILogger logger)
    {
        Path = path.ThrowIfNull();
        _fileSystem = fileSystem.ThrowIfNull();

        if (!fileSystem.File.Exists(path))
            throw new ArgumentException($"Project file with path {path} was not found");

        Accessor = DotnetProjectFile.Create(Path, _fileSystem, logger);
    }

    public bool SupportModification()
    {
        return Accessor.IsSdkFormat();
    }

    public void Save(XmlDocumentSyntaxFormatter syntaxFormatter)
    {
        _fileSystem.File.WriteAllText(Path, Accessor.ToXmlString(syntaxFormatter));
    }
}