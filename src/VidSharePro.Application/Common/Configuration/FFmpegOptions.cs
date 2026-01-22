namespace VidSharePro.Application.Common.Configuration;

public class FFmpegOptions
{
    public const string Section = "FFmpeg";
    public string ExecutablePath { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
}