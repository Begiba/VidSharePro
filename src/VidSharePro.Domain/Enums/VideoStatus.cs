    // VidSharePro.Domain/Enums/VideoStatus.cs
    namespace VidSharePro.Domain.Enums;

    public enum VideoStatus
{
    Uploading = 0,
    PendingValidation = 1,
    Processing = 2, // <--- Add this
    Ready = 3,
    Failed = 4,
    Deleted = 5
}