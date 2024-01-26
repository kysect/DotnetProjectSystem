using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Language.Xml;
using System.IO.Abstractions;

namespace Kysect.DotnetProjectSystem.SolutionModification;

public interface IXmlProjectFileModifyStrategy<TSyntax>
    where TSyntax : SyntaxNode
{
    IReadOnlyCollection<TSyntax> SelectNodeForModify(XmlDocumentSyntax document);
    SyntaxNode ApplyChanges(TSyntax syntax);
}

public static class SolutionItemNameConstants
{
    public const string DirectoryBuildProps = "Directory.Build.props";
    public const string DirectoryPackagesProps = "Directory.Packages.props";
}

public class DotnetSolutionModifier
{
    public IReadOnlyCollection<DotnetProjectModifier> Projects { get; }
    public DotnetPropsModifier DirectoryBuildPropsModifier { get; }
    public DotnetPropsModifier DirectoryPackagePropsModifier { get; }

    public static DotnetSolutionModifier Create(
        string solutionPath,
        IFileSystem fileSystem,
        ILogger logger,
        SolutionFileContentParser solutionFileParser,
        XmlDocumentSyntaxFormatter xmlDocumentSyntaxFormatter)
    {
        solutionPath.ThrowIfNull();
        fileSystem.ThrowIfNull();
        logger.ThrowIfNull();
        solutionFileParser.ThrowIfNull();

        IFileInfo fileInfo = fileSystem.FileInfo.New(solutionPath);
        fileInfo.Directory.ThrowIfNull();

        var directoryBuildPropsModifier = new DotnetPropsModifier(
            fileSystem.Path.Combine(fileInfo.Directory.FullName, SolutionItemNameConstants.DirectoryBuildProps),
            fileSystem,
            logger,
            xmlDocumentSyntaxFormatter);
        var directoryPackagePropsModifier = new DotnetPropsModifier(
            fileSystem.Path.Combine(fileInfo.Directory.FullName, SolutionItemNameConstants.DirectoryPackagesProps),
            fileSystem,
            logger,
            xmlDocumentSyntaxFormatter);

        string solutionFileContent = fileSystem.File.ReadAllText(solutionPath);
        IReadOnlyCollection<DotnetProjectFileDescriptor> projectFileDescriptors = solutionFileParser.ParseSolutionFileContent(solutionFileContent);
        var projects = new List<DotnetProjectModifier>();

        foreach (DotnetProjectFileDescriptor projectFileDescriptor in projectFileDescriptors)
        {
            string projectFullPath = fileSystem.Path.Combine(fileInfo.Directory.FullName, projectFileDescriptor.ProjectPath);
            var projectModifier = new DotnetProjectModifier(projectFullPath, fileSystem, logger, xmlDocumentSyntaxFormatter);
            bool supportModification = projectModifier.SupportModification();
            if (!supportModification)
                logger.LogWarning("Project {Path} use legacy csproj format and will be skipped.", projectModifier.Path);
            else
                projects.Add(projectModifier);
        }

        return new DotnetSolutionModifier(projects, directoryBuildPropsModifier, directoryPackagePropsModifier);
    }

    public DotnetSolutionModifier(IReadOnlyCollection<DotnetProjectModifier> projects, DotnetPropsModifier directoryBuildPropsModifier, DotnetPropsModifier directoryPackagePropsModifier)
    {
        Projects = projects;
        DirectoryBuildPropsModifier = directoryBuildPropsModifier;
        DirectoryPackagePropsModifier = directoryPackagePropsModifier;
    }

    public void Save()
    {
        DirectoryBuildPropsModifier.Save();
        DirectoryPackagePropsModifier.Save();

        foreach (DotnetProjectModifier projectModifier in Projects)
            projectModifier.Save();
    }
}

public class DotnetProjectModifier
{
    private readonly XmlDocumentSyntaxFormatter _xmlDocumentSyntaxFormatter;

    public string Path { get; }
    public DotnetProjectFile Accessor { get; }

    private readonly IFileSystem _fileSystem;

    public DotnetProjectModifier(string path, IFileSystem fileSystem, ILogger logger, XmlDocumentSyntaxFormatter xmlDocumentSyntaxFormatter)
    {
        _xmlDocumentSyntaxFormatter = xmlDocumentSyntaxFormatter;
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

    public void Save()
    {
        _fileSystem.File.WriteAllText(Path, Accessor.ToXmlString(_xmlDocumentSyntaxFormatter));
    }
}

public class DotnetPropsModifier
{
    private readonly string _path;
    private readonly IFileSystem _fileSystem;
    private readonly Lazy<DotnetProjectFile> _fileAccessor;
    private XmlDocumentSyntaxFormatter _xmlDocumentSyntaxFormatter;
    public DotnetProjectFile Accessor => _fileAccessor.Value;

    public DotnetPropsModifier(string path, IFileSystem fileSystem, ILogger logger, XmlDocumentSyntaxFormatter xmlDocumentSyntaxFormatter)
    {
        _path = path;
        _fileSystem = fileSystem;
        _xmlDocumentSyntaxFormatter = xmlDocumentSyntaxFormatter;
        _fileAccessor = new Lazy<DotnetProjectFile>(() => DotnetProjectFile.Create(_path, _fileSystem, logger));
    }

    public void Save()
    {
        if (!_fileAccessor.IsValueCreated)
            return;

        _xmlDocumentSyntaxFormatter = new XmlDocumentSyntaxFormatter();
        _fileSystem.File.WriteAllText(_path, _fileAccessor.Value.ToXmlString(_xmlDocumentSyntaxFormatter));
    }
}

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