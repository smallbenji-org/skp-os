namespace SKP.OS.Backend;

public class Settings
{
    public string ConnectionString { get; set; } = "";

    public CheckInSettings CheckIn { get; set; } = new();
}

public class CheckInSettings
{
    /// <summary>
    /// CIDR subnets a student's client IP must be inside to be allowed to check in.
    /// Empty means the check is disabled.
    /// </summary>
    public string[] AllowedSubnets { get; set; } = [];
}
