namespace ChildGuard.Core.Configuration;

public class PolicyRule
{
    // Days of week as names: e.g., ["Mon","Tue","Wed","Thu","Fri","Sat","Sun"]; empty = all days
    public string[] Days { get; set; } = Array.Empty<string>();
    // Time window local, format HH:mm - both inclusive, overnight supported via Start>End
    public string Start { get; set; } = "00:00";
    public string End { get; set; } = "23:59";
    // If Allow has entries, only these are allowed; Block always blocks.
    public string[] Allow { get; set; } = Array.Empty<string>();
    public string[] Block { get; set; } = Array.Empty<string>();
}
