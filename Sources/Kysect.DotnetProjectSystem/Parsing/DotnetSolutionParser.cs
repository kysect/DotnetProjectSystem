using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tools;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.DotnetProjectSystem.Parsing;

public class DotnetSolutionParser
{
    private readonly IFileSystem _fileSystem;
    private readonly SolutionFileContentParser _solutionFileParser;
    private readonly ILogger _logger;

    public DotnetSolutionParser(IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem;
        _solutionFileParser = new SolutionFileContentParser();
        _logger = logger;
    }

    public DotnetSolutionDescriptor Parse(string filePath)
    {
        filePath.ThrowIfNull(nameof(filePath));

        _logger.LogInformation("Parsing solution {solutionPath}", filePath);
        var solutionFileFullPath = _fileSystem.FileInfo.New(filePath).FullName;

        if (!_fileSystem.File.Exists(solutionFileFullPath))
            throw new DotnetProjectSystemException($"Cannot parse solution {solutionFileFullPath}. File not found.");

        IDirectoryInfo? solutionDirectory = _fileSystem.Directory.GetParent(solutionFileFullPath);
        if (solutionDirectory is null)
            throw new DotnetProjectSystemException("Cannot get solution parent directory");

        string slnFileContent = _fileSystem.File.ReadAllText(solutionFileFullPath);

        var projectFileDescriptors = _solutionFileParser.ParseSolutionFileContent(slnFileContent).ToList();
        var projects = new Dictionary<string, DotnetProjectFile>();
        foreach (DotnetProjectFileDescriptor? projectFileDescriptor in projectFileDescriptors)
        {
            string projectFullPath = _fileSystem.Path.Combine(solutionDirectory.FullName, projectFileDescriptor.FileSystemPath);

            _logger.LogTrace("Parsing project {path}", projectFullPath);
            string csprojContent = _fileSystem.File.ReadAllText(projectFullPath);
            projects[projectFullPath] = DotnetProjectFile.Create(csprojContent);
        }

        _logger.LogInformation("Solution parsed and contains {projectCount}", projectFileDescriptors.Count);
        return new DotnetSolutionDescriptor(solutionFileFullPath, projects);
    }
}