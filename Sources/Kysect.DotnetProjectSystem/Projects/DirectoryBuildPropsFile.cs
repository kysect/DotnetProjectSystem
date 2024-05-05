namespace Kysect.DotnetProjectSystem.Projects;

public class DirectoryBuildPropsFile
{
    public DotnetProjectFile File { get; }

    public static DirectoryBuildPropsFile CreateEmpty()
    {
        return new DirectoryBuildPropsFile(DotnetProjectFile.CreateEmpty());
    }

    public DirectoryBuildPropsFile(string fileContent) : this(DotnetProjectFile.Create(fileContent))
    {
    }

    public DirectoryBuildPropsFile(DotnetProjectFile file)
    {
        File = file;
    }

    public void SetArtifactsOutput(bool artifactOutput)
    {
        File.Properties.SetProperty("UseArtifactsOutput", artifactOutput);
    }

    public bool ArtifactsOutputEnabled()
    {
        return File.Properties.FindBooleanProperty("UseArtifactsOutput") ?? false;
    }
}