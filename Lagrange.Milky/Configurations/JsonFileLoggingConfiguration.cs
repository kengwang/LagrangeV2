namespace Lagrange.Milky.Configurations;

/// <summary>Controls the optional rolling JSON Lines log file.</summary>
public sealed class JsonFileLoggingConfiguration
{
    /// <summary>Whether JSON file logging is enabled. Defaults to false.</summary>
    public bool Enabled { get; set; }

    /// <summary>Log path, relative to the application's content root unless absolute.</summary>
    public string Path { get; set; } = "logs/milky-.jsonl";

    /// <summary>Maximum number of rolling files retained, including size-based rolls.</summary>
    public int RetainedFileCountLimit { get; set; } = 7;

    /// <summary>File size in bytes at which an additional rolling file is created.</summary>
    public long FileSizeLimitBytes { get; set; } = 100 * 1024 * 1024;
}
