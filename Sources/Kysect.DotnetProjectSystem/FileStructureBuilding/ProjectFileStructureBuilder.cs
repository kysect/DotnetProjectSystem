using Kysect.CommonLib.BaseTypes.Extensions;
using System.IO.Abstractions;

namespace Kysect.DotnetProjectSystem.FileStructureBuilding;

public class ProjectFileStructureBuilder
{
    private readonly string _projectFileContent;
    private readonly List<SolutionFileInfoElement> _files;

    public string ProjectName { get; }

    public ProjectFileStructureBuilder(string projectName, string projectFileContent)
    {
        ProjectName = projectName;
        _projectFileContent = projectFileContent;

        _files = new List<SolutionFileInfoElement>();
    }

    public ProjectFileStructureBuilder AddEmptyFile(params string[] path)
    {
        return AddFile(new SolutionFileInfoElement(path, string.Empty));
    }

    public ProjectFileStructureBuilder AddFile(IReadOnlyCollection<string> path, string content)
    {
        return AddFile(new SolutionFileInfoElement(path, content));
    }

    public ProjectFileStructureBuilder AddFile(SolutionFileInfoElement fileStructureElement)
    {
        _files.Add(fileStructureElement);
        return this;
    }

    public void Save(IFileSystem fileSystem, string rootPath)
    {
        fileSystem.ThrowIfNull();

        fileSystem.EnsureDirectoryExists(fileSystem.Path.Combine(rootPath, ProjectName));
        string csprojPath = fileSystem.Path.Combine(rootPath, ProjectName, $"{ProjectName}.csproj");
        fileSystem.File.WriteAllText(csprojPath, _projectFileContent);

        foreach (var solutionFileInfo in _files)
        {
            string[] fileFullPathParts = [rootPath, ProjectName, .. solutionFileInfo.Path];
            string fileFullPath = fileSystem.Path.Combine(fileFullPathParts);
            IFileInfo fileInfo = fileSystem.FileInfo.New(fileFullPath);
            fileSystem.EnsureContainingDirectoryExists(fileInfo);

            fileSystem.File.WriteAllText(fileFullPath, solutionFileInfo.Content);
        }
    }
}