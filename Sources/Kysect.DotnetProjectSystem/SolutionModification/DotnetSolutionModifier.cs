using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using System.IO.Abstractions;

namespace Kysect.DotnetProjectSystem.SolutionModification;

public class DotnetSolutionModifier
{
    private readonly IFileInfo _solutionPath;
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
        IFileInfo solutionPath)
    {
        _projects = projects;
        _directoryBuildPropsModifier = directoryBuildPropsModifier;
        _directoryPackagePropsModifier = directoryPackagePropsModifier;
        _fileSystem = fileSystem;
        _solutionPath = solutionPath;
    }

    public void Save(XmlDocumentSyntaxFormatter syntaxFormatter)
    {
        _solutionPath.Directory.ThrowIfNull();

        if (_directoryBuildPropsModifier is not null)
        {
            string directoryBuildPropsPath = _fileSystem.Path.Combine(_solutionPath.Directory.FullName, SolutionItemNameConstants.DirectoryBuildProps);
            _fileSystem.File.WriteAllText(directoryBuildPropsPath, _directoryBuildPropsModifier.File.ToXmlString(syntaxFormatter));
        }

        if (_directoryPackagePropsModifier is not null)
        {
            string directoryPackagesPropsPath = _fileSystem.Path.Combine(_solutionPath.Directory.FullName, SolutionItemNameConstants.DirectoryPackagesProps);
            _fileSystem.File.WriteAllText(directoryPackagesPropsPath, _directoryPackagePropsModifier.File.ToXmlString(syntaxFormatter));
        }

        foreach (KeyValuePair<string, DotnetCsprojFile> projectModifier in _projects)
        {
            _fileSystem.File.WriteAllText(projectModifier.Key, projectModifier.Value.File.ToXmlString(syntaxFormatter));
        }
    }
}