using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tools;
using Kysect.DotnetProjectSystem.Xml;

namespace Kysect.DotnetProjectSystem.SolutionModification;

public class CentralPackageManagementMigrator
{
    private readonly XmlDocumentSyntaxFormatter _formatter;

    public CentralPackageManagementMigrator(XmlDocumentSyntaxFormatter formatter)
    {
        _formatter = formatter;
    }

    public void Migrate(DotnetSolutionModifier solutionModifier)
    {
        solutionModifier.ThrowIfNull();

        DirectoryPackagesPropsFile directoryPackagesPropsFile = solutionModifier.GetOrCreateDirectoryPackagePropsModifier();
        if (directoryPackagesPropsFile.GetCentralPackageManagement())
            throw new DotnetProjectSystemException("Cannot migrate solution to CPM. Solution already use CPM.");

        directoryPackagesPropsFile.SetCentralPackageManagement(true);

        solutionModifier.Save(_formatter);
    }
}