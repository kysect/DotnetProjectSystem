using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using System.IO.Abstractions;

namespace Kysect.DotnetProjectSystem.SolutionModification;

public class DotnetSolutionModifier
{
    private readonly IFileInfo _solutionPath;
    private readonly XmlDocumentSyntaxFormatter _syntaxFormatter;
    private readonly IFileSystem _fileSystem;
    private DirectoryBuildPropsFile? _directoryBuildPropsModifier;
    private DirectoryPackagesPropsFile? _directoryPackagePropsModifier;
    private readonly Dictionary<string, DotnetCsprojFile> _projects;

    public IReadOnlyCollection<KeyValuePair<string, DotnetCsprojFile>> Projects => _projects;

    public DirectoryBuildPropsFile GetOrCreateDirectoryBuildPropsModifier()
    {
        _directoryBuildPropsModifier ??= new DirectoryBuildPropsFile(DotnetProjectFile.CreateEmpty());
        return _directoryBuildPropsModifier;
    }

    public DirectoryPackagesPropsFile GetOrCreateDirectoryPackagePropsModifier()
    {
        _directoryPackagePropsModifier ??= new DirectoryPackagesPropsFile(DotnetProjectFile.CreateEmpty());
        return _directoryPackagePropsModifier;
    }

    public DotnetSolutionModifier(
        Dictionary<string, DotnetCsprojFile> projects,
        DirectoryBuildPropsFile? directoryBuildPropsModifier,
        DirectoryPackagesPropsFile? directoryPackagePropsModifier,
        IFileSystem fileSystem,
        IFileInfo solutionPath,
        XmlDocumentSyntaxFormatter syntaxFormatter)
    {
        _projects = projects;
        _directoryBuildPropsModifier = directoryBuildPropsModifier;
        _directoryPackagePropsModifier = directoryPackagePropsModifier;
        _fileSystem = fileSystem;
        _solutionPath = solutionPath;
        _syntaxFormatter = syntaxFormatter;
    }

    public void Save()
    {
        _solutionPath.Directory.ThrowIfNull();

        if (_directoryBuildPropsModifier is not null)
        {
            string directoryBuildPropsPath = _fileSystem.Path.Combine(_solutionPath.Directory.FullName, SolutionItemNameConstants.DirectoryBuildProps);
            _fileSystem.File.WriteAllText(directoryBuildPropsPath, _directoryBuildPropsModifier.File.ToXmlString(_syntaxFormatter));
        }

        if (_directoryPackagePropsModifier is not null)
        {
            string directoryPackagesPropsPath = _fileSystem.Path.Combine(_solutionPath.Directory.FullName, SolutionItemNameConstants.DirectoryPackagesProps);
            _fileSystem.File.WriteAllText(directoryPackagesPropsPath, _directoryPackagePropsModifier.File.ToXmlString(_syntaxFormatter));
        }

        foreach (KeyValuePair<string, DotnetCsprojFile> projectModifier in _projects)
        {
            _fileSystem.File.WriteAllText(projectModifier.Key, projectModifier.Value.File.ToXmlString(_syntaxFormatter));
        }
    }
}