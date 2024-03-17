using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.FileSystem;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using System.IO.Abstractions;

namespace Kysect.DotnetProjectSystem.FileStructureBuilding;

public class SolutionFileStructureBuilder
{
    private readonly IFileSystem _fileSystem;
    private readonly XmlDocumentSyntaxFormatter _syntaxFormatter;
    private readonly string _solutionName;
    private readonly List<ProjectFileStructureBuilder> _projects;
    private readonly List<SolutionStructureElement> _files;
    private DirectoryBuildPropsFile? _directoryBuildPropsFile;
    private DirectoryPackagesPropsFile? _directoryPackagesPropsFile;

    public SolutionFileStructureBuilder(IFileSystem fileSystem, XmlDocumentSyntaxFormatter syntaxFormatter, string solutionName)
    {
        _fileSystem = fileSystem;
        _syntaxFormatter = syntaxFormatter;
        _solutionName = solutionName;

        _projects = new List<ProjectFileStructureBuilder>();
        _files = new List<SolutionStructureElement>();
    }

    public SolutionFileStructureBuilder AddProject(ProjectFileStructureBuilder project)
    {
        _projects.Add(project);
        return this;
    }

    public SolutionFileStructureBuilder AddFile(IReadOnlyCollection<string> path, string content)
    {
        return AddFile(new SolutionStructureElement(path, content));
    }

    public SolutionFileStructureBuilder AddFile(SolutionStructureElement fileStructureElement)
    {
        _files.Add(fileStructureElement);
        return this;
    }

    public SolutionFileStructureBuilder AddDirectoryBuildProps(string content)
    {
        return AddDirectoryBuildProps(new DirectoryBuildPropsFile(DotnetProjectFile.Create(content)));
    }

    public SolutionFileStructureBuilder AddDirectoryBuildProps(DirectoryBuildPropsFile directoryBuildPropsFile)
    {
        _directoryBuildPropsFile = directoryBuildPropsFile;
        return this;
    }

    public SolutionFileStructureBuilder AddDirectoryPackagesProps(string content)
    {
        return AddDirectoryPackagesProps(new DirectoryPackagesPropsFile(DotnetProjectFile.Create(content)));
    }

    public SolutionFileStructureBuilder AddDirectoryPackagesProps(DirectoryPackagesPropsFile directoryPackagesPropsFile)
    {
        _directoryPackagesPropsFile = directoryPackagesPropsFile;
        return this;
    }

    public void Save(string solutionDirectory)
    {
        string solutionFileContent = CreateSolutionFile(_fileSystem);

        DirectoryExtensions.EnsureDirectoryExists(_fileSystem, solutionDirectory);
        _fileSystem.File.WriteAllText(_fileSystem.Path.Combine(solutionDirectory, $"{_solutionName}.sln"), solutionFileContent);

        if (_directoryBuildPropsFile is not null)
            AddFile([SolutionItemNameConstants.DirectoryBuildProps], _directoryBuildPropsFile.File.ToXmlString(_syntaxFormatter));

        if (_directoryPackagesPropsFile is not null)
            AddFile([SolutionItemNameConstants.DirectoryPackagesProps], _directoryPackagesPropsFile.File.ToXmlString(_syntaxFormatter));

        foreach (SolutionStructureElement? solutionFileInfo in _files)
        {
            string partialFilePath = _fileSystem.Path.Combine(solutionFileInfo.Path.ToArray());
            _fileSystem.File.WriteAllText(_fileSystem.Path.Combine(solutionDirectory, partialFilePath), solutionFileInfo.Content);
        }

        foreach (ProjectFileStructureBuilder projectBuilder in _projects)
            projectBuilder.Save(_fileSystem, solutionDirectory, _syntaxFormatter);
    }

    public string CreateSolutionFile(IFileSystem fileSystem)
    {
        fileSystem.ThrowIfNull();

        var solutionFileStringBuilder = new SolutionFileStringBuilder();

        foreach (ProjectFileStructureBuilder? projectBuilder in _projects)
            solutionFileStringBuilder.AddProject(projectBuilder.ProjectName, fileSystem.Path.Combine(projectBuilder.ProjectName, $"{projectBuilder.ProjectName}.csproj"));

        return solutionFileStringBuilder.Build();
    }
}