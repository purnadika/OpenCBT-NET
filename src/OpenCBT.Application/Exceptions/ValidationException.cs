namespace OpenCBT.Application.Exceptions;

public class ValidationException : Exception
{
    public object[] Args { get; }

    public ValidationException(string messageKey, params object[] args) : base(messageKey)
    {
        Args = args;
    }
}
