using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Tools;

namespace Kysect.DotnetProjectSystem.Projects;

public static class DotnetProjectFileExtensions
{
    public static bool? FindBooleanProperty(this DotnetProjectFile file, string propertyName)
    {
        file.ThrowIfNull();
        propertyName.ThrowIfNull();

        DotnetProjectProperty? property = file.FindProperty(propertyName);
        if (property is null)
            return null;

        if (!bool.TryParse(property.Value.Value, out bool result))
            throw new DotnetProjectSystemException($"Cannot parse project property value {property} to bool");

        return result;
    }

    public static bool IsEnableDefaultItems(this DotnetProjectFile file)
    {
        file.ThrowIfNull();

        bool? enableDefaultItems = file.FindBooleanProperty(DotnetProjectFileConstant.EnableDefaultItems);
        if (enableDefaultItems is not null)
            return enableDefaultItems.Value;

        return file.IsSdkFormat();
    }
}