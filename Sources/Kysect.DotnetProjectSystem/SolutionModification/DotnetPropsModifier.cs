using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.DotnetProjectSystem.SolutionModification;

public class DotnetPropsModifier
{
    private readonly string _path;
    private readonly IFileSystem _fileSystem;
    private readonly Lazy<DotnetProjectFile> _fileAccessor;
    public DotnetProjectFile Accessor => _fileAccessor.Value;

    public DotnetPropsModifier(string path, IFileSystem fileSystem, ILogger logger)
    {
        _path = path;
        _fileSystem = fileSystem;
        _fileAccessor = new Lazy<DotnetProjectFile>(() => DotnetProjectFile.Create(_path, _fileSystem, logger));
    }

    public void Save(XmlDocumentSyntaxFormatter xmlDocumentSyntaxFormatter)
    {
        if (!_fileAccessor.IsValueCreated)
            return;

        _fileSystem.File.WriteAllText(_path, _fileAccessor.Value.ToXmlString(xmlDocumentSyntaxFormatter));
    }
}