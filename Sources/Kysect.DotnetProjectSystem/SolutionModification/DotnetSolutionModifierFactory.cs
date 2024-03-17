using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using System.IO.Abstractions;

namespace Kysect.DotnetProjectSystem.SolutionModification;

public class DotnetSolutionModifierFactory
{
    private readonly IFileSystem _fileSystem;
    private readonly SolutionFileContentParser _solutionFileParser;
    private readonly XmlDocumentSyntaxFormatter _syntaxFormatter;

    public DotnetSolutionModifierFactory(IFileSystem fileSystem, SolutionFileContentParser solutionFileParser, XmlDocumentSyntaxFormatter syntaxFormatter)
    {
        _fileSystem = fileSystem;
        _solutionFileParser = solutionFileParser;
        _syntaxFormatter = syntaxFormatter;
    }

    public DotnetSolutionModifier Create(string solutionPath)
    {
        solutionPath.ThrowIfNull();

        IFileInfo solutionFileInfo = _fileSystem.FileInfo.New(solutionPath);

        DirectoryBuildPropsFile? directoryBuildPropsModifier = TryCreateDirectoryBuildPropsFile(solutionFileInfo);
        DirectoryPackagesPropsFile? directoryPackagesPropsFile = TryCreateDirectoryPackagesPropsFile(solutionFileInfo);
        Dictionary<string, DotnetCsprojFile> projects = CreateProjectModifiers(solutionFileInfo);

        return new DotnetSolutionModifier(
            projects,
            directoryBuildPropsModifier,
            directoryPackagesPropsFile,
            _fileSystem,
            solutionFileInfo,
            _syntaxFormatter);
    }

    private DirectoryBuildPropsFile? TryCreateDirectoryBuildPropsFile(IFileInfo solutionFileInfo)
    {
        solutionFileInfo.Directory.ThrowIfNull();

        string path = _fileSystem.Path.Combine(solutionFileInfo.Directory.FullName, SolutionItemNameConstants.DirectoryBuildProps);
        if (!_fileSystem.File.Exists(path))
            return null;

        DotnetProjectFile dotnetProjectFile = Create(path, _fileSystem);
        return new DirectoryBuildPropsFile(dotnetProjectFile);
    }

    private DirectoryPackagesPropsFile? TryCreateDirectoryPackagesPropsFile(IFileInfo solutionFileInfo)
    {
        solutionFileInfo.Directory.ThrowIfNull();

        string path = _fileSystem.Path.Combine(solutionFileInfo.Directory.FullName, SolutionItemNameConstants.DirectoryPackagesProps);
        if (!_fileSystem.File.Exists(path))
            return null;

        DotnetProjectFile dotnetProjectFile = Create(path, _fileSystem);
        return new DirectoryPackagesPropsFile(dotnetProjectFile);
    }

    private Dictionary<string, DotnetCsprojFile> CreateProjectModifiers(IFileInfo solutionFileInfo)
    {
        solutionFileInfo.Directory.ThrowIfNull();

        string solutionFileContent = _fileSystem.File.ReadAllText(solutionFileInfo.FullName);
        IReadOnlyCollection<DotnetProjectFileDescriptor> projectFileDescriptors = _solutionFileParser.ParseSolutionFileContent(solutionFileContent);

        var projects = new Dictionary<string, DotnetCsprojFile>();
        foreach (DotnetProjectFileDescriptor projectFileDescriptor in projectFileDescriptors)
        {
            string projectFullPath = _fileSystem.Path.Combine(solutionFileInfo.Directory.FullName, projectFileDescriptor.FileSystemPath);
            if (!_fileSystem.File.Exists(projectFullPath))
                throw new DotnetProjectSystemException($"Project file with path {projectFullPath} was not found");

            var dotnetProjectFile = Create(projectFullPath, _fileSystem);
            var projectModifier = new DotnetCsprojFile(dotnetProjectFile);
            projects.Add(projectFullPath, projectModifier);
        }

        return projects;
    }

    private static DotnetProjectFile Create(string path, IFileSystem fileSystem)
    {
        path.ThrowIfNull();
        fileSystem.ThrowIfNull();

        string csprojContent =
            fileSystem.File.Exists(path)
                ? fileSystem.File.ReadAllText(path)
                : string.Empty;

        return DotnetProjectFile.Create(csprojContent);
    }
}