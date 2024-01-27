﻿namespace Kysect.DotnetProjectSystem.Projects;

public class DirectoryBuildPropsFile
{
    public DotnetProjectFile File { get; }

    public DirectoryBuildPropsFile(DotnetProjectFile file)
    {
        File = file;
    }

    public bool ArtifactsOutputEnabled()
    {
        return File.FindBooleanProperty("UseArtifactsOutput") ?? false;
    }
}