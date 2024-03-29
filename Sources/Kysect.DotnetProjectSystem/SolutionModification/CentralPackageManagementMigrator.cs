﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tools;
using Microsoft.Extensions.Logging;

namespace Kysect.DotnetProjectSystem.SolutionModification;

public class CentralPackageManagementMigrator
{
    private readonly ILogger _logger;

    public CentralPackageManagementMigrator(ILogger logger)
    {
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
        foreach (var package in nugetPackages)
            directoryPackagesPropsFile.Versions.AddPackageVersion(package.Name, package.Version);

        _logger.LogTrace("Apply changes to *.csproj files");
        foreach (KeyValuePair<string, DotnetCsprojFile> csprojFile in solutionModifier.Projects)
        {
            foreach ((string name, string? _) in csprojFile.Value.File.PackageReferences.GetPackageReferences())
            {
                csprojFile.Value.File.PackageReferences.RemoveVersion(name);
            }
        }

        _logger.LogTrace("Saving solution files");
        solutionModifier.Save();
    }

    private IReadOnlyCollection<ProjectPackageVersion> CollectNugetIncludes(DotnetSolutionModifier modifier)
    {
        var nugetVersions = new List<ProjectPackageVersion>();

        foreach (KeyValuePair<string, DotnetCsprojFile> dotnetProjectModifier in modifier.Projects)
        {
            foreach (ProjectPackageReference packageReferences in dotnetProjectModifier.Value.File.PackageReferences.GetPackageReferences())
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