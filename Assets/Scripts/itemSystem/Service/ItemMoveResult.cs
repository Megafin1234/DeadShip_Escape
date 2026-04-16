using System;

[Serializable]
public class ItemMoveResult
{
    public bool Success;
    public string Message;

    public static ItemMoveResult Ok(string message = "")
    {
        return new ItemMoveResult
        {
            Success = true,
            Message = message
        };
    }

    public static ItemMoveResult Fail(string message)
    {
        return new ItemMoveResult
        {
            Success = false,
            Message = message
        };
    }
}