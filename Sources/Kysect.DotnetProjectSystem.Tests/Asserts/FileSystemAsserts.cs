using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.Asserts;

public class FileSystemAsserts
{
    private readonly MockFileSystem _fileSystem;

    public FileSystemAsserts(MockFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public FileSystemFileAsserts File(params string[] pathParts)
    {
        return new FileSystemFileAsserts(_fileSystem, _fileSystem.Path.Combine(pathParts));
    }
}