﻿using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Xml;
using Microsoft.Language.Xml;

namespace Kysect.DotnetProjectSystem.Tests.Projects;

public class DotnetProjectFileTests
{
    private readonly XmlDocumentSyntaxFormatter _formatter;

    public DotnetProjectFileTests()
    {
        _formatter = new XmlDocumentSyntaxFormatter();
    }

    [Fact]
    public void CreateEmpty_ReturnXmlWithProjectNode()
    {
        string expected = """
                          <Project>
                          </Project>
                          """;

        var sut = DotnetProjectFile.CreateEmpty();

        string actual = sut.ToXmlString(_formatter);

        actual.Should().Be(expected);
    }

    [Fact]
    public void Create_FromStringWithProjectNode_ReturnXmlWithProjectNode()
    {
        string expected = """
                          <Project>
                          </Project>
                          """;

        var sut = DotnetProjectFile.Create(expected);

        string actual = sut.ToXmlString(_formatter);

        actual.Should().Be(expected);
    }

    [Fact]
    public void Create_FromEmptyString_ReturnXmlWithProjectNode()
    {
        string expected = """
                          <Project>
                          </Project>
                          """;

        var sut = DotnetProjectFile.Create(string.Empty);

        string actual = sut.ToXmlString(_formatter);

        actual.Should().Be(expected);
    }

    [Fact]
    public void Create_FromStringWithXmlTag_ThrowExceptionAboutInvalidTag()
    {
        var argumentException = Assert.Throws<ArgumentException>(() =>
        {
            DotnetProjectFile.Create("<xml/>");
        });

        argumentException.Should().NotBeNull();
    }

    [Fact]
    public void Create_NotProjectNode_ThrowException()
    {
        const string input = "<SomeNode />";

        FluentActions
            .Invoking(() => DotnetProjectFile.Create(input))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("XML root must be Project");
    }

    [Fact]
    public void GetProjectNode_ForEmptyNode_ReturnProjectNode()
    {
        var sut = DotnetProjectFile.CreateEmpty();

        IXmlElement projectNode = sut.GetProjectNode();

        projectNode.Name.Should().Be("Project");
    }

    [Fact]
    public void GetOrAddPropertyGroup_ForEmptyString_ReturnStringWithPropertyGroup()
    {
        string expected = """
                          <Project>
                            <PropertyGroup></PropertyGroup>
                          </Project>
                          """;
        var sut = DotnetProjectFile.CreateEmpty();

        IXmlElementSyntax propertyGroupNode = sut.GetOrAddPropertyGroup();
        string actual = sut.ToXmlString(_formatter);

        actual.Should().Be(expected);
        propertyGroupNode.Name.Should().Be("PropertyGroup");
    }

    [Fact]
    public void GetOrAddPropertyGroup_ForXmlWithPropertyGroup_DocumentMustNotBeChanged()
    {
        string expected = """
                          <Project>
                            <PropertyGroup>
                            </PropertyGroup>
                          </Project>
                          """;

        var sut = DotnetProjectFile.Create(expected);
        var propertyGroupNode = sut.GetOrAddPropertyGroup();
        var actual = sut.ToXmlString(_formatter);

        actual.Should().Be(expected);
        propertyGroupNode.Name.Should().Be("PropertyGroup");
    }

    [Fact]
    public void GetOrAddItemGroup_ForEmptyString_ReturnStringWithItemGroup()
    {
        string expected = """
                          <Project>
                            <ItemGroup>
                            </ItemGroup>
                          </Project>
                          """;

        var sut = DotnetProjectFile.Create(expected);
        var propertyGroupNode = sut.GetOrAddItemGroup();
        var actual = sut.ToXmlString(_formatter);

        actual.Should().Be(expected);
        propertyGroupNode.Name.Should().Be("ItemGroup");
    }

    [Fact]
    public void IsSdkFormat_ForEmptyFile_ReturnTrue()
    {
        string projectContent = """
                                <Project>
                                  <PropertyGroup>
                                  </PropertyGroup>
                                </Project>
                                """;

        var sut = DotnetProjectFile.Create(projectContent);

        sut.IsSdkFormat().Should().BeTrue();
    }

    [Fact]
    public void IsSdkFormat_ForLegacyFormat_ReturnFalse()
    {
        var sut = DotnetProjectFile.CreateLegacyFormat();

        sut.IsSdkFormat().Should().BeFalse();
    }

    [Fact]
    public void GetItems_ForProjectWithCompileNode_ReturnItem()
    {
        const string content = """
                               <Project>
                                 <ItemGroup>
                                   <Compile Include="File.cs" />
                                   <Compile Include="File2.cs" />
                                 </ItemGroup>
                               </Project>
                               """;

        var sut = DotnetProjectFile.Create(content);

        IReadOnlyCollection<DotnetProjectItem> compileItems = sut.GetItems("Compile");

        compileItems.Should().BeEquivalentTo([
            new DotnetProjectItem("Compile", "File.cs"),
            new DotnetProjectItem("Compile", "File2.cs")
        ]);
    }

    [Fact]
    public void AddCompileItem_ForEmptyProject_ReturnExpectedContent()
    {
        const string expected = """
                                <Project>
                                  <ItemGroup>
                                    <Compile Include="File1.cs" />
                                  </ItemGroup>
                                </Project>
                                """;

        DotnetProjectFile projectFile = DotnetProjectFile
            .CreateEmpty()
            .AddCompileItem("File1.cs");

        projectFile.ToXmlString(_formatter).Should().Be(expected);
    }
}