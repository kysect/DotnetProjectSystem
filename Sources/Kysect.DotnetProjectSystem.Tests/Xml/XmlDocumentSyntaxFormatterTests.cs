using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Tests.Xml;

public class XmlDocumentSyntaxFormatterTests
{
    private readonly XmlDocumentSyntaxFormatter _formatter;

    public XmlDocumentSyntaxFormatterTests()
    {
        _formatter = new XmlDocumentSyntaxFormatter();
    }

    [Fact]
    public void Format_FormatterDocument_ReturnStringWithoutChanges()
    {
        var input = """
                          <Project>
                            <Node1>
                              <Node2 />
                            </Node1>
                          </Project>
                          """;

        var expected = """
                    <Project>
                      <Node1>
                        <Node2 />
                      </Node1>
                    </Project>
                    """;

        Validate(input, expected);
    }

    [Fact]
    public void Format_NodeWithoutPrecedingTrivia_ReturnStringWithCorrectTrivia()
    {
        var expected = """
                          <Project>
                            <Node1>
                              <Node2 />
                            </Node1>
                          </Project>
                          """;

        var input = """
                       <Project>
                       <Node1>
                       <Node2 />
                       </Node1>
                       </Project>
                       """;

        Validate(input, expected);
    }

    [Fact]
    public void Format_NodesInOneLine_ReturnStringWithCorrectTrivia()
    {
        var expected = """
                       <Project>
                         <Node1>
                           <Node2 />
                         </Node1>
                       </Project>
                       """;

        var input = """
                    <Project><Node1><Node2 /></Node1></Project>
                    """;

        Validate(input, expected);
    }

    [Fact]
    public void Format_MultipleChild_ReturnStringWithCorrectTrivia()
    {
        var expected = """
                       <Project>
                         <Node1 />
                         <Node2 />
                         <Node3 />
                       </Project>
                       """;

        var input = """
                    <Project>
                    <Node1 />
                    <Node2 />
                    <Node3 />
                    </Project>
                    """;

        Validate(input, expected);
    }

    [Fact]
    public void Format_NodeWithContent_NodeInOneLine()
    {
        var expected = """
                       <Project>
                         <Node1>Some text</Node1>
                       </Project>
                       """;

        var input = """
                    <Project><Node1>Some text</Node1>
                    </Project>
                    """;

        Validate(input, expected);
    }

    [Fact]
    public void Format_XmlElementWithAttributes_SpacesBetweenElements()
    {
        var expected = """
                       <Project>
                         <Node Attribute1="Value" Attribute2="Value" />
                       </Project>
                       """;

        var input = """
                    <Project>
                      <Node Attribute1="Value"Attribute2="Value"/>
                    </Project>
                    """;

        Validate(input, expected);
    }

    [Fact]
    public void Format_XmlElementWithAttributesAndSpaceBeforeEnding_SpacesBetweenElements()
    {
        var expected = """
                       <Project>
                         <Node Attribute1="Value" Attribute2="Value" />
                       </Project>
                       """;

        var input = """
                    <Project>
                      <Node Attribute1="Value"Attribute2="Value" />
                    </Project>
                    """;

        Validate(input, expected);
    }

    [Fact]
    public void Format_XmlEmptyTagWithoutAttribute_ExpectedResult()
    {
        var expected = """
                       <Project>
                         <Node />
                       </Project>
                       """;

        var input = """
                    <Project>
                      <Node />
                    </Project>
                    """;

        Validate(input, expected);
    }

    [Fact]
    public void Format_FileWithComment_ExpectNoChanges()
    {
        var input = """
                    <Project>
                      <!-- First -->
                      <PropertyGroup>
                        <IncludeSymbols>true</IncludeSymbols>
                      </PropertyGroup>
                    
                      <!-- Second -->
                      <PropertyGroup>
                        <IncludeSymbols>true</IncludeSymbols>
                      </PropertyGroup>
                    </Project>
                    """;

        Validate(input, input);
    }

    private void Validate(string input, string expected)
    {
        XmlDocumentSyntax document = Parser.ParseText(input);
        XmlDocumentSyntax actual = _formatter.Format(document);

        actual.ToFullString().Should().Be(expected);
    }
}