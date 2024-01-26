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

    private void Validate(string input, string expected)
    {
        XmlDocumentSyntax document = Parser.ParseText(input);
        XmlDocumentSyntax actual = _formatter.Format(document);

        actual.ToFullString().Should().Be(expected);
    }
}