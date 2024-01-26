using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Xml;
using System.IO.Abstractions;

namespace Kysect.DotnetProjectSystem.FileStructureBuilding;

public class SolutionFileStructureBuilder
{
    private readonly string _solutionName;
    private readonly List<ProjectFileStructureBuilder> _projects;
    private readonly List<SolutionFileInfoElement> _files;

    public SolutionFileStructureBuilder(string solutionName)
    {
        _solutionName = solutionName;

        _projects = new List<ProjectFileStructureBuilder>();
        _files = new List<SolutionFileInfoElement>();
    }

    public SolutionFileStructureBuilder AddProject(ProjectFileStructureBuilder project)
    {
        _projects.Add(project);
        return this;
    }

    public SolutionFileStructureBuilder AddFile(IReadOnlyCollection<string> path, string content)
    {
        return AddFile(new SolutionFileInfoElement(path, content));
    }

    public SolutionFileStructureBuilder AddFile(SolutionFileInfoElement fileStructureElement)
    {
        _files.Add(fileStructureElement);
        return this;
    }

    public void Save(IFileSystem fileSystem, string rootPath)
    {
        fileSystem.ThrowIfNull();

        string solutionFileContent = CreateSolutionFile(fileSystem);
        fileSystem.File.WriteAllText(fileSystem.Path.Combine(rootPath, $"{_solutionName}.sln"), solutionFileContent);

        foreach (var solutionFileInfo in _files)
        {
            string partialFilePath = fileSystem.Path.Combine(solutionFileInfo.Path.ToArray());
            fileSystem.File.WriteAllText(fileSystem.Path.Combine(rootPath, partialFilePath), solutionFileInfo.Content);
        }

        foreach (ProjectFileStructureBuilder projectBuilder in _projects)
            projectBuilder.Save(fileSystem, rootPath, new XmlDocumentSyntaxFormatter());
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