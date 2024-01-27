using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Tests.Xml;

public class ExtendedSyntaxFactoryTests
{
    [Fact]
    public void XmlElement_ReturnExpectedString()
    {
        const string expected = """
                                <ElementName></ElementName>
                                """;
        XmlElementSyntax xmlElementSyntax = ExtendedSyntaxFactory.XmlElement("ElementName");

        xmlElementSyntax.ToFullString().Should().Be(expected);
    }

    [Fact]
    public void XmlEmptyElement_ReturnExpectedString()
    {
        const string expected = """
                                <ElementName />
                                """;
        XmlEmptyElementSyntax xmlElementSyntax = ExtendedSyntaxFactory.XmlEmptyElement("ElementName");

        xmlElementSyntax.ToFullString().Should().Be(expected);
    }
}