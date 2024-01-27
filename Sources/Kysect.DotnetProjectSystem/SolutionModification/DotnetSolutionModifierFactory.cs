using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Microsoft.Language.Xml;
using System.IO.Abstractions;

namespace Kysect.DotnetProjectSystem.SolutionModification;

public class DotnetSolutionModifierFactory
{
    private readonly IFileSystem _fileSystem;
    private readonly SolutionFileContentParser _solutionFileParser;

    public DotnetSolutionModifierFactory(IFileSystem fileSystem, SolutionFileContentParser solutionFileParser)
    {
        _fileSystem = fileSystem;
        _solutionFileParser = solutionFileParser;
    }

    public DotnetSolutionModifier Create(string solutionPath)
    {
        solutionPath.ThrowIfNull();

        IFileInfo solutionFileInfo = _fileSystem.FileInfo.New(solutionPath);

        DotnetPropsModifier? directoryBuildPropsModifier = TryCreatePropsFile(solutionFileInfo, SolutionItemNameConstants.DirectoryBuildProps);
        DotnetPropsModifier? directoryPackagePropsModifier = TryCreatePropsFile(solutionFileInfo, SolutionItemNameConstants.DirectoryPackagesProps);
        Dictionary<string, DotnetProjectModifier> projects = CreateProjectModifiers(solutionFileInfo);

        return new DotnetSolutionModifier(projects, directoryBuildPropsModifier, directoryPackagePropsModifier, _fileSystem, solutionFileInfo);
    }

    private DotnetPropsModifier? TryCreatePropsFile(IFileInfo solutionFileInfo, string fileName)
    {
        solutionFileInfo.Directory.ThrowIfNull();

        string path = _fileSystem.Path.Combine(solutionFileInfo.Directory.FullName, fileName);
        if (!_fileSystem.File.Exists(path))
            return null;

        var dotnetProjectFile = Create(path, _fileSystem);
        return new DotnetPropsModifier(dotnetProjectFile);
    }

    private Dictionary<string, DotnetProjectModifier> CreateProjectModifiers(IFileInfo solutionFileInfo)
    {
        solutionFileInfo.Directory.ThrowIfNull();

        string solutionFileContent = _fileSystem.File.ReadAllText(solutionFileInfo.FullName);
        IReadOnlyCollection<DotnetProjectFileDescriptor> projectFileDescriptors = _solutionFileParser.ParseSolutionFileContent(solutionFileContent);

        var projects = new Dictionary<string, DotnetProjectModifier>();
        foreach (DotnetProjectFileDescriptor projectFileDescriptor in projectFileDescriptors)
        {
            string projectFullPath = _fileSystem.Path.Combine(solutionFileInfo.Directory.FullName, projectFileDescriptor.ProjectPath);
            if (!_fileSystem.File.Exists(projectFullPath))
                throw new ArgumentException($"Project file with path {projectFullPath} was not found");

            var dotnetProjectFile = Create(projectFullPath, _fileSystem);
            var projectModifier = new DotnetProjectModifier(dotnetProjectFile);
            projects.Add(projectFullPath, projectModifier);
        }

        return projects;
    }

    public static DotnetProjectFile Create(string path, IFileSystem fileSystem)
    {
        path.ThrowIfNull();
        fileSystem.ThrowIfNull();

        string csprojContent =
            fileSystem.File.Exists(path)
                ? fileSystem.File.ReadAllText(path)
                : string.Empty;

        XmlDocumentSyntax root = Parser.ParseText(csprojContent);
        return new DotnetProjectFile(root);
    }
}