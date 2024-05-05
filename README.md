# Kysect.DotnetProjectSystem

[![codecov](https://codecov.io/gh/kysect/DotnetProjectSystem/graph/badge.svg?token=eRI09WyDsH)](https://codecov.io/gh/kysect/DotnetProjectSystem)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Fkysect%2FDotnetProjectSystem%2Fmaster)](https://dashboard.stryker-mutator.io/reports/github.com/kysect/DotnetProjectSystem/master)

DotnetProjectSystem is a nuget package for working with .sln, .csproj and .props files.

### Main features

- Creating solution and project files structure from code, fluent API
- Parsing solution and project files
- Typed wrappers for reading and modifying solution and project files
- API for modifying solution and project files. Implementation of strategy for migration solution to Central Package Management
- Support of [TestableIO.System.IO.Abstractions](https://github.com/TestableIO/System.IO.Abstractions) for testing

### Creating solution and project files structure

- Task: create solution file, project files and Directory.Build.props file.
- Use case: need to create sample solution for test proposes (on real file system or MockFileSystem).

Code sample for creating solution and project files structure:

```csharp
var factory = new SolutionFileStructureBuilderFactory(fileSystem, syntaxFormatter);

var project =
    new ProjectFileStructureBuilder("FirstProject")
        .SetContent("""
                    <Project>
                      <PropertyGroup>
                        <TargetFramework>net8.0</TargetFramework>
                      </PropertyGroup>
                    </Project>
                    """)
        .AddFile(["Models", "MyModel.cs"], "using System;");

var directoryBuildPropsFile = DirectoryBuildPropsFile.CreateEmpty();
directoryBuildPropsFile.File.Properties.SetProperty("Company", "Kysect");

factory.Create("NewSolution")
    .AddProject(project)
    .AddDirectoryBuildProps(directoryBuildPropsFile)
    .Save(@"D:\Repositories\NewSolution");
```

will create this file structure:

```
D:\Repositories\NewSolution\
|- FirstProject/
   |- FirstProject.csproj
   |- Models
      |- MyModel.cs
|- Directory.Build.props
|- NewSolution.sln
```

### Parsing solution and project files

Nuget provide API for parsing .sln and .csproj files:

```csharp
string solutionFilePath = ...;
var parser = new DotnetSolutionParser(_fileSystem, logger);
var result = parser.Parse(solutionFilePath);
```

Parse method return model DotnetSolutionDescriptor that contains information about projects. Project represented by DotnetProjectFile model that provide access to project properties and items.

### Typed wrappers for reading and modifying solution and project files

Some samples for working with DotnetProjectFile:

```csharp
DotnetProjectFile project = ...;
var packageReferences = project.PackageReferences.GetPackageReferences();
// packageReferences = { { "System.Text.Json", "8.0.0" }, { "Microsoft.Extensions.Logging", "8.0.0" } }

project.PackageReferences.SetPackageReference("System.Text.Json", "9.0.0");
// packageReferences = { { "System.Text.Json", "9.0.0" }, { "Microsoft.Extensions.Logging", "8.0.0" }

project.PackageReferences.AddPackageReference("Kysect.Editorconfig", "1.0.0");

var implicitUsings = project.Properties.GetProperty("ImplicitUsings");
// implicitUsings = true
project.Properties.SetProperty("ImplicitUsings", "false");
// implicitUsings = false
```

### API for modifying solution and project files

Nuget provide API for modification of parsed solution. NuGet package shipped with implementation of strategy for migration solution to Central Package Management as sample.

```csharp
string solutionFilePath = ...;
var factory = new DotnetSolutionModifierFactory(_fileSystem, _solutionFileContentParser, _formatter);
var solutionModifier = _solutionModifierFactory.Create(solutionFilePath);
var migrator = new CentralPackageManagementMigrator(logger);

migrator.Migrate(solutionModifier);
```

### Support of TestableIO.System.IO.Abstractions

TestableIO.System.IO.Abstractions is NuGet that provide abstraction for file system. This abstraction can be used for testing. Nuget provide implementation of IFileSystem for TestableIO.System.IO.Abstractions. Kysect.DotnetProjectSystem fully support this abstraction.

## Used by

Currently main use case for this library is https://github.com/kysect/Zeya - tool for managing .NET projects and modification them. You can check it for more examples of using this library.