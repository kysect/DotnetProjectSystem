﻿using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.Tools;
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
        string projectContent = """
                                <Project ToolsVersion="15.0">
                                </Project>
                                """;

        var sut = DotnetProjectFile.Create(projectContent);

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
    public void GetProperty_ForEmptyProject_ThrowException()
    {
        var content = """
                      <Project>
                        <PropertyGroup>
                        </PropertyGroup>
                      </Project>
                      """;

        var sut = DotnetProjectFile.Create(content);

        Assert.Throws<DotnetProjectSystemException>(() =>
        {
            sut.GetProperty("TargetFramework");
        });
    }

    [Fact]
    public void GetProperty_ForProjectWithDuplicatedProperties_ThrowException()
    {
        var content = """
                      <Project>
                        <PropertyGroup>
                          <TargetFramework>net8.0</TargetFramework>
                          <TargetFramework>net8.0</TargetFramework>
                        </PropertyGroup>
                      </Project>
                      """;

        var sut = DotnetProjectFile.Create(content);

        Assert.Throws<DotnetProjectSystemException>(() =>
        {
            sut.GetProperty("TargetFramework");
        });
    }

    [Fact]
    public void GetProperty_ForProjectWithProperty_ReturnValue()
    {
        var content = """
                      <Project>
                        <PropertyGroup>
                          <TargetFramework>net8.0</TargetFramework>
                        </PropertyGroup>
                      </Project>
                      """;

        var sut = DotnetProjectFile.Create(content);

        DotnetProjectProperty compileItems = sut.GetProperty("TargetFramework");

        compileItems.Should().Be(new DotnetProjectProperty("TargetFramework", "net8.0"));
    }
}