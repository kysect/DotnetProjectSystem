using Kysect.CommonLib.BaseTypes.Extensions;
using System.IO.Abstractions;

namespace Kysect.DotnetProjectSystem.FileStructureBuilding;

// TODO: Move to Kysect.CommonLib
public static class FileSystemExtensions
{
    public static void EnsureContainingDirectoryExists(this IFileSystem fileSystem, IFileInfo fileInfo)
    {
        fileSystem.ThrowIfNull();
        fileInfo.ThrowIfNull();

        if (fileInfo.Directory == null)
            throw new ArgumentException($"Cannot get directory for path {fileInfo.FullName}");

        fileSystem.EnsureDirectoryExists(fileInfo.Directory.FullName);
    }

    public static void EnsureDirectoryExists(this IFileSystem fileSystem, string path)
    {
        fileSystem.ThrowIfNull();
        path.ThrowIfNull();

        if (!fileSystem.Directory.Exists(path))
            fileSystem.Directory.CreateDirectory(path);
    }
}