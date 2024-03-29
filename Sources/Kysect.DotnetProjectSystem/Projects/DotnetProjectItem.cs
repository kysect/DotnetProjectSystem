﻿namespace Kysect.DotnetProjectSystem.Projects;

public record struct DotnetProjectItem(string Group, string Include);
public record struct ProjectPackageReference(string Name, string? Version);