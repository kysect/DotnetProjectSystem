namespace Kysect.DotnetProjectSystem.Tools;

public class DotnetProjectSystemException : Exception
{
    public DotnetProjectSystemException(string message) : base(message)
    {
    }

    public DotnetProjectSystemException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public DotnetProjectSystemException()
    {
    }
}