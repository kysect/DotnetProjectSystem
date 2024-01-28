using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.FileSystem;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using System.IO.Abstractions;

namespace Kysect.DotnetProjectSystem.FileStructureBuilding;

public class SolutionFileStructureBuilder
{
    private readonly string _solutionName;
    private readonly List<ProjectFileStructureBuilder> _projects;
    private readonly List<SolutionStructureElement> _files;

    public SolutionFileStructureBuilder(string solutionName)
    {
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
        return AddFile([SolutionItemNameConstants.DirectoryBuildProps], content);
    }

    public SolutionFileStructureBuilder AddDirectoryPackagesProps(string content)
    {
        return AddFile([SolutionItemNameConstants.DirectoryPackagesProps], content);
    }

    public void Save(IFileSystem fileSystem, string solutionDirectory, XmlDocumentSyntaxFormatter syntaxFormatter)
    {
        fileSystem.ThrowIfNull();

        string solutionFileContent = CreateSolutionFile(fileSystem);

        DirectoryExtensions.EnsureDirectoryExists(fileSystem, solutionDirectory);
        fileSystem.File.WriteAllText(fileSystem.Path.Combine(solutionDirectory, $"{_solutionName}.sln"), solutionFileContent);

        foreach (var solutionFileInfo in _files)
        {
            string partialFilePath = fileSystem.Path.Combine(solutionFileInfo.Path.ToArray());
            fileSystem.File.WriteAllText(fileSystem.Path.Combine(solutionDirectory, partialFilePath), solutionFileInfo.Content);
        }

        foreach (ProjectFileStructureBuilder projectBuilder in _projects)
            projectBuilder.Save(fileSystem, solutionDirectory, syntaxFormatter);
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