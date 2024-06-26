﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.FileSystem;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Xml;
using System.IO.Abstractions;

namespace Kysect.DotnetProjectSystem.FileStructureBuilding;

public class ProjectFileStructureBuilder
{
    private DotnetProjectFile _projectFile;
    private readonly List<SolutionStructureElement> _files;

    public string ProjectName { get; }

    public ProjectFileStructureBuilder(string projectName)
        : this(projectName, DotnetProjectFile.CreateEmpty())
    {
    }

    public ProjectFileStructureBuilder(string projectName, string projectFileContent)
        : this(projectName, DotnetProjectFile.Create(projectFileContent))
    {
    }

    public ProjectFileStructureBuilder(string projectName, DotnetProjectFile projectFileContent)
    {
        ProjectName = projectName;
        _projectFile = projectFileContent;
        _files = new List<SolutionStructureElement>();
    }

    public ProjectFileStructureBuilder SetContent(string projectFileContent)
    {
        return SetContent(DotnetProjectFile.Create(projectFileContent));
    }

    public ProjectFileStructureBuilder SetContent(DotnetProjectFile projectFileContent)
    {
        _projectFile = projectFileContent;
        return this;
    }

    public ProjectFileStructureBuilder AddEmptyFile(params string[] path)
    {
        return AddFile(new SolutionStructureElement(path, string.Empty));
    }

    public ProjectFileStructureBuilder AddFile(IReadOnlyCollection<string> path, string content)
    {
        return AddFile(new SolutionStructureElement(path, content));
    }

    public ProjectFileStructureBuilder AddFile(SolutionStructureElement fileStructureElement)
    {
        _files.Add(fileStructureElement);
        return this;
    }

    public void Save(IFileSystem fileSystem, string rootPath, XmlDocumentSyntaxFormatter syntaxFormatter)
    {
        fileSystem.ThrowIfNull();

        string csprojDirectoryPath = fileSystem.Path.Combine(rootPath, ProjectName);
        string csprojPath = fileSystem.Path.Combine(csprojDirectoryPath, $"{ProjectName}.csproj");

        DirectoryExtensions.EnsureDirectoryExists(fileSystem, csprojDirectoryPath);
        string csprojContent = _projectFile.ToXmlString(syntaxFormatter);
        fileSystem.File.WriteAllText(csprojPath, csprojContent);

        foreach (SolutionStructureElement? solutionFileInfo in _files)
        {
            string[] fileFullPathParts = [csprojDirectoryPath, .. solutionFileInfo.Path];
            string fileFullPath = fileSystem.Path.Combine(fileFullPathParts);
            IFileInfo fileInfo = fileSystem.FileInfo.New(fileFullPath);
            DirectoryExtensions.EnsureParentDirectoryExists(fileSystem, fileInfo);

            fileSystem.File.WriteAllText(fileFullPath, solutionFileInfo.Content);
        }
    }
}