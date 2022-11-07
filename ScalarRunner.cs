using Microsoft.Data.SqlClient;

public class ScalarRunner : Runner
{

    public ScalarRunner(Arguments settings, IDictionary<string, string> extras) : base(settings, extras) { }

    public override void Run()
    {
        Connect();
        using SqlCommand command = new(GetSql(), connection);
        object result = command.ExecuteScalar();
        Console.Out.WriteLine(result);
    }

}