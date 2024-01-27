# Kysect.DotnetProjectSystem

[![codecov](https://codecov.io/gh/kysect/DotnetProjectSystem/graph/badge.svg?token=eRI09WyDsH)](https://codecov.io/gh/kysect/DotnetProjectSystem)

DotnetProjectSystem is a nuget package for working with .sln, .csproj and .props files: parsing, modifying and generating.

## Generating

This code samples:

```csharp
new SolutionFileStructureBuilder(solutionName)
    .AddProject(
        new ProjectFileStructureBuilder("Project")
            .SetContent("<Project></Project>"))
    .AddFile(new SolutionFileInfo(["Directory.Build.props"], "<Project></Project>"))
    .Save(_fileSystem, "C:\\Repositories\\", _syntaxFormatter);
```

will create this file structure:

```
- ./
    - Project/
        - Project.csproj
    - MySolution.sln
    - Directory.Build.props
```

## Parsing

Nuget provide API for parsing .sln and .csproj files:

```csharp
string solutionFilePath = ...;
DotnetSolutionParser parser = new DotnetSolutionParser(_fileSystem, logger);
DotnetSolutionDescriptor result = parser.ParseContent(solutionFilePath);

// result.FilePath == C:\Solution\Solution.sln
// result.Projects ==
// {
//    { "C:\Solution\Project.csproj", <DotnetProjectFile> }
// }
```

## Modification

Nuget provide API for modification of parsed solution. Sample of modification;

```csharp
syntaxFormatter = new XmlDocumentSyntaxFormatter();
DotnetSolutionModifier solutionModifier = solutionModifierFactory.Create("Solution.sln");

solutionModifier
    .GetOrCreateDirectoryPackagePropsModifier()
    .SetCentralPackageManagement(true);

solutionModifier.Save(syntaxFormatter)
```

This code will add `<ManagePackageVersionsCentrally>` to `Directory.Package.props` file.

For this modification was introduces strategy that describe modification:

```csharp
public class SetTargetFrameworkModifyStrategy(string value) : IXmlProjectFileModifyStrategy<XmlElementSyntax>
{
    public IReadOnlyCollection<XmlElementSyntax> SelectNodeForModify(XmlDocumentSyntax document)
    {
        document.ThrowIfNull();

        return document
            .GetNodesByName("TargetFramework")
            .OfType<XmlElementSyntax>()
            .ToList();
    }

    public SyntaxNode ApplyChanges(XmlElementSyntax syntax)
    {
        syntax.ThrowIfNull();

        XmlTextSyntax content = SyntaxFactory.XmlText(SyntaxFactory.XmlTextLiteralToken(value, null, null));
        return syntax.ReplaceNode(syntax.Content.Single(), content);
    }
}
```

And this strategy applied to solutions:

```csharp
DotnetSolutionModifier solutionModifier = solutionModifierFactory.Create("Solution.sln");

foreach (DotnetProjectModifier solutionModifierProject in solutionModifier.Projects)
    solutionModifierProject.File.UpdateDocument(new SetTargetFrameworkModifyStrategy("net9.0"));

solutionModifier.Save(syntaxFormatter);
```
