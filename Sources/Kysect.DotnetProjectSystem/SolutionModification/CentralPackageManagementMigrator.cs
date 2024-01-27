using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Extensions.Logging;

namespace Kysect.DotnetProjectSystem.SolutionModification;

public class CentralPackageManagementMigrator
{
    private readonly XmlDocumentSyntaxFormatter _formatter;
    private readonly ILogger _logger;

    public CentralPackageManagementMigrator(XmlDocumentSyntaxFormatter formatter, ILogger logger)
    {
        _formatter = formatter;
        _logger = logger;
    }

    public void Migrate(DotnetSolutionModifier solutionModifier)
    {
        solutionModifier.ThrowIfNull();

        DirectoryPackagesPropsFile directoryPackagesPropsFile = solutionModifier.GetOrCreateDirectoryPackagePropsModifier();
        if (directoryPackagesPropsFile.GetCentralPackageManagement())
            throw new DotnetProjectSystemException("Cannot migrate solution to CPM. Solution already use CPM.");

        IReadOnlyCollection<ProjectPackageVersion> nugetPackages = CollectNugetIncludes(solutionModifier);
        nugetPackages = SelectDistinctPackages(nugetPackages);
        _logger.LogDebug("Collect {Count} added Nuget packages to projects", nugetPackages.Count);

        _logger.LogTrace("Apply changes to {DirectoryPackageFile} file", SolutionItemNameConstants.DirectoryPackagesProps);
        directoryPackagesPropsFile.SetCentralPackageManagement(true);

        _logger.LogDebug("Adding package versions to {DirectoryPackageFile}", SolutionItemNameConstants.DirectoryPackagesProps);
        foreach ((string name, string version) in nugetPackages)
            directoryPackagesPropsFile.AddPackageVersion(name, version);

        _logger.LogTrace("Apply changes to *.csproj files");
        foreach (DotnetCsprojFile csprojFile in solutionModifier.Projects)
        {
            foreach ((string name, string? _) in csprojFile.File.PackageReferences.GetPackageReferences())
            {
                csprojFile.File.PackageReferences.UpdatePackageReference(name, null);
            }
        }

        _logger.LogTrace("Saving solution files");
        solutionModifier.Save(_formatter);
    }

    private IReadOnlyCollection<ProjectPackageVersion> CollectNugetIncludes(DotnetSolutionModifier modifier)
    {
        var nugetVersions = new List<ProjectPackageVersion>();

        foreach (var dotnetProjectModifier in modifier.Projects)
        {
            foreach (var packageReferences in dotnetProjectModifier.File.PackageReferences.GetPackageReferences())
            {
                if (packageReferences.Version is null)
                    continue;

                nugetVersions.Add(new ProjectPackageVersion(packageReferences.Name, packageReferences.Version));
            }
        }

        return nugetVersions;
    }

    private IReadOnlyCollection<ProjectPackageVersion> SelectDistinctPackages(IReadOnlyCollection<ProjectPackageVersion> packages)
    {
        List<ProjectPackageVersion> distinctPackages = new List<ProjectPackageVersion>();

        foreach (IGrouping<string, ProjectPackageVersion> nugetVersions in packages.GroupBy(p => p.Name))
        {
            if (nugetVersions.Count() == 1)
            {
                distinctPackages.Add(nugetVersions.Single());
                continue;
            }

            List<string> versions = nugetVersions.Select(n => n.Version).Distinct().ToList();
            if (versions.Count == 1)
            {
                distinctPackages.Add(new ProjectPackageVersion(nugetVersions.Key, versions.Single()));
                continue;
            }

            _logger.LogWarning("Nuget {Package} added to projects with different versions: {Versions}", nugetVersions.Key, versions.ToSingleString());
            Version lastVersion = versions
                .Select(Version.Parse)
                .Max();

            distinctPackages.Add(new ProjectPackageVersion(nugetVersions.Key, lastVersion.ToString()));
        }

        return distinctPackages
            .OrderBy(p => p.Name)
            .ToList();
    }
}