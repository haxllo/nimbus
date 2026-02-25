namespace Nimbus.Core.Services;

public enum FileOperationErrorCode
{
    None = 0,
    InvalidInput = 1,
    NotFound = 2,
    Conflict = 3,
    AccessDenied = 4,
    IoError = 5,
    Cancelled = 6,
    Unknown = 7
}

public sealed class FileOperationResult
{
    private FileOperationResult(bool isSuccess, FileOperationErrorCode errorCode, string message)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        Message = message;
    }

    public bool IsSuccess { get; }

    public FileOperationErrorCode ErrorCode { get; }

    public string Message { get; }

    public static FileOperationResult Success(string message) =>
        new(isSuccess: true, FileOperationErrorCode.None, message);

    public static FileOperationResult Failure(FileOperationErrorCode errorCode, string message) =>
        new(isSuccess: false, errorCode, message);
}
