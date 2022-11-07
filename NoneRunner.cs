
using Microsoft.Data.SqlClient;

public class NoneRunner : Runner
{

    public NoneRunner(Arguments settings, IDictionary<string, string> extras) : base(settings, extras) { }

    public override void Run()
    {
        Connect();
        using SqlCommand command = new(GetSql(), connection);
        int result = command.ExecuteNonQuery();
        Console.Out.WriteLine(result);
    }

}