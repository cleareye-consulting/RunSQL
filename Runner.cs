using Microsoft.Data.SqlClient;

public abstract class Runner : IDisposable
{
    protected readonly Arguments settings;
    private readonly IDictionary<string, string> extras;
    private bool disposedValue;
    protected SqlConnection? connection;

    public Runner(Arguments settings, IDictionary<string, string> extras)
    {
        this.settings = settings;
        this.extras = extras;
    }

    protected void Connect()
    {
        SqlConnectionStringBuilder connectionStringBuilder = new();
        connectionStringBuilder.DataSource = settings.Port != default ? $"{settings.Server}:{settings.Port}" : settings.Server;
        connectionStringBuilder.InitialCatalog = settings.Database;
        connectionStringBuilder.UserID = settings.UserId;
        connectionStringBuilder.Password = settings.Password;
        connectionStringBuilder.Encrypt = false; //https://github.com/dotnet/SqlClient/issues/1479
        connectionStringBuilder.TrustServerCertificate = true;
        connection = new(connectionStringBuilder.ConnectionString);
        connection.Open();
    }

    protected string GetSql()
    {
        string sql = settings.Sql ?? File.ReadAllText(settings.In!);
        foreach (string key in extras.Keys)
        {
            sql = sql.Replace($"%{key}%", extras[key]);
        }
        return sql;
    }

    public abstract void Run();

    public static Runner GetRunner(Arguments settings, IDictionary<string, string> extras)
    {
        if (settings.ResultType is null)
        {
            return new QueryRunner(settings, extras);
        }
        if (settings.ResultType.Equals("query", StringComparison.CurrentCultureIgnoreCase))
        {
            return new QueryRunner(settings, extras);
        }
        if (settings.ResultType.Equals("scalar", StringComparison.CurrentCultureIgnoreCase))
        {
            return new ScalarRunner(settings, extras);
        }
        if (settings.ResultType.Equals("none", StringComparison.CurrentCultureIgnoreCase))
        {
            return new NoneRunner(settings, extras);
        }
        throw new ArgumentException("ResultType must be QUERY, SCALAR, or NONE if specified", nameof(settings));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                connection?.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}