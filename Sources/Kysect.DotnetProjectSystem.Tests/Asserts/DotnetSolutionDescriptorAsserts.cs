using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Xml;

namespace Kysect.DotnetProjectSystem.Tests.Asserts;

public class DotnetSolutionDescriptorAsserts
{
    private readonly XmlDocumentSyntaxFormatter _formatter;

    public DotnetSolutionDescriptorAsserts(XmlDocumentSyntaxFormatter formatter)
    {
        _formatter = formatter;
    }

    public bool Equals(DotnetSolutionDescriptor? actual, DotnetSolutionDescriptor? expected)
    {
        if (ReferenceEquals(actual, expected))
            return true;
        if (actual is null)
            return false;
        if (expected is null)
            return false;
        if (actual.GetType() != expected.GetType())
            return false;

        actual.FilePath.Should().Be(expected.FilePath);
        actual.Projects.Count.Should().Be(expected.Projects.Count);

        foreach ((string? key, DotnetProjectFile? value) in actual.Projects)
        {
            string expectedProjectContent = expected.Projects[key].ToXmlString(_formatter);
            string actualProjectContent = actual.Projects[key].ToXmlString(_formatter);
            expectedProjectContent.Should().Be(actualProjectContent);
        }

        return actual.FilePath == expected.FilePath && actual.Projects.Equals(expected.Projects);
    }
}