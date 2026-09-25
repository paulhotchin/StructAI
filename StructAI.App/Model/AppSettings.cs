namespace StructAI.Model;

public class AppSettings
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string TitleMobile { get; set; } = "";
    public string PageTitle { get; set; } = "";
    public string ProjectType { get; set; } = "";
    public List<AppProject> Projects { get; set; } = new();
}

public class AppProject
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
    public string Enabled { get; set; } = "N";

    public bool IsEnabled =>
        string.Equals(Enabled, "Y", StringComparison.OrdinalIgnoreCase);
}
