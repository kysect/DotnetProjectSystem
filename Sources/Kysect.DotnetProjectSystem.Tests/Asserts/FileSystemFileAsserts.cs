using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetProjectSystem.Tests.Asserts;

public class FileSystemFileAsserts
{
    private readonly MockFileSystem _fileSystem;
    private readonly string _path;

    public FileSystemFileAsserts(MockFileSystem fileSystem, string path)
    {
        _fileSystem = fileSystem;
        _path = path;
    }

    public FileSystemFileAsserts ShouldExists()
    {
        _fileSystem.File.Exists(_path).Should().BeTrue();
        return this;
    }

    public FileSystemFileAsserts ShouldHaveContent(string content)
    {
        _fileSystem.File.ReadAllText(_path).Should().BeEquivalentTo(content);
        return this;
    }

    public FileSystemFileAsserts ShouldHaveEmptyContent()
    {
        _fileSystem.File.ReadAllText(_path).Should().BeEmpty();
        return this;
    }
}