namespace JustRPG.Exceptions;

public class UserInteractionException : Exception
{
    public UserInteractionException(string message) : base("[]" + message)
    {
        
    }
}

public class UserInteractionWarning: Exception
{
    public UserInteractionWarning(string message) : base("<>"+message)
    {
        
    }
}
