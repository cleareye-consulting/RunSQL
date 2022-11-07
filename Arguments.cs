public class Arguments
{
    public string Server { get; set; } = "";
    public int? Port { get; set; }
    public string Database { get; set; } = "";
    public string UserId { get; set; } = "";
    public string Password { get; set; } = "";
    public string? Sql { get; set; }
    public string? In { get; set; }
    public string? Out { get; set; }
    public string? Format { get; set; }
    public string? ResultType { get; set; }
}