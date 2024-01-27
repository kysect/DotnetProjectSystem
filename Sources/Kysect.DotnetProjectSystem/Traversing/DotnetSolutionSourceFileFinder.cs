using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tools;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.DotnetProjectSystem.Traversing;

public record DotnetSolutionPaths(string SolutionFileFullPath, IReadOnlyCollection<DotnetProjectPaths> ProjectPaths);
public record DotnetProjectPaths(string ProjectFileFullPath, IReadOnlyCollection<string> SourceFileFullPaths);

public class DotnetSolutionSourceFileFinder
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public DotnetSolutionSourceFileFinder(IFileSystem fileSystem, ILogger logger)
    {
        _logger = logger;
        _fileSystem = fileSystem;
    }

    public DotnetSolutionPaths FindSourceFiles(DotnetSolutionDescriptor solutionDescriptor)
    {
        solutionDescriptor.ThrowIfNull();

        _logger.LogInformation("Extract source file paths for solution {path}", solutionDescriptor.FilePath);
        _fileSystem.File.Exists(solutionDescriptor.FilePath);

        var projectPaths = new List<DotnetProjectPaths>();

        foreach (KeyValuePair<string, DotnetProjectFile> descriptorProject in solutionDescriptor.Projects)
        {
            string? filePath = descriptorProject.Key;
            DotnetProjectFile projectFile = descriptorProject.Value;

            IFileInfo projectFileInfo = _fileSystem.FileInfo.New(filePath);
            if (projectFileInfo.Directory is null)
                throw new DotnetProjectSystemException($"Cannot get project directory for {filePath}");

            _logger.LogInformation("Adding files from csproj");
            List<string> projectFileFullPaths = projectFile
                .GetItems("Compile")
                .Select(p => _fileSystem.Path.Combine(projectFileInfo.Directory.FullName, p.Include))
                .ToList();

            if (projectFile.IsEnableDefaultItems())
            {
                string binDirectoryPath = _fileSystem.Path.Combine(projectFileInfo.Directory.FullName, "bin");
                string objDirectoryPath = _fileSystem.Path.Combine(projectFileInfo.Directory.FullName, "obj");

                _logger.LogInformation("Default items enabled. Trying to add files in directory");
                var defaultItems = _fileSystem.Directory
                    .EnumerateFiles(projectFileInfo.Directory.FullName, "*", SearchOption.AllDirectories)
                    .Where(p => p != projectFileInfo.FullName)
                    .Where(p => !p.StartsWith(binDirectoryPath))
                    .Where(p => !p.StartsWith(objDirectoryPath))
                    .ToList();

                projectFileFullPaths.AddRange(defaultItems);
            }

            projectPaths.Add(new DotnetProjectPaths(filePath, projectFileFullPaths));
        }

        return new DotnetSolutionPaths(solutionDescriptor.FilePath, projectPaths);
    }
}