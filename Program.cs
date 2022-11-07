
//if in file, read into SQL variable (fail on failed read)
//if out file not writable, fail
//build connection string; if no user ID and password, specify trusted connection
//connect to database (fail on failure)
//create command from SQL
//if result type flag Q or not specified:
//  read results into a data table
//  if out file specified, write results to file in specified out format (default to CSV)
//  if no out file specified, write results to stdout in specified out format (default to ASCII table)
//  if read fails, write error to stderr and exit with nonzero
//if result type flag S:
//  execute scalar
//  write result to stdout
//  if read fails, write error to stderr and exit with nonzero
//if result type flag N
//  execute non-query
//  write affected row count to stdout
//  if operation fails, write error to stderr and exit with nozero
//exit with zero

using Microsoft.Data.SqlClient;

public static class Program
{
    public static int Main(string[] args)
    {
        var (settings, extras) = Utility.GetCommandLineArgs<Arguments>(args);
        if (settings is null)
        {
            PrintUsage(Console.Out);
            return 1;
        }
        if (string.IsNullOrWhiteSpace(settings.Server) || string.IsNullOrWhiteSpace(settings.Database))
        {
            PrintUsage(Console.Out);
            return 1;
        }
        if (string.IsNullOrWhiteSpace(settings.UserId) && !string.IsNullOrWhiteSpace(settings.Password))
        {
            //Can't specify password without user ID
            PrintUsage(Console.Out);
            return 1;
        }
        if (string.IsNullOrWhiteSpace(settings.Sql) && string.IsNullOrWhiteSpace(settings.In)
        || !string.IsNullOrWhiteSpace(settings.Sql) && !string.IsNullOrWhiteSpace(settings.In))
        {
            //Have to specify either Sql or In
            PrintUsage(Console.Out);
            return 1;
        }
        if (settings.Format is not null
            && !settings.Format.Equals("json", StringComparison.CurrentCultureIgnoreCase)
            && !settings.Format.Equals("csv", StringComparison.CurrentCultureIgnoreCase))
        {
            //Format must be JSON or CSV if specified
            PrintUsage(Console.Out);
            return 1;
        }
        if (settings.ResultType is not null
            && !settings.ResultType.Equals("query", StringComparison.CurrentCultureIgnoreCase)
            && !settings.ResultType.Equals("scalar", StringComparison.CurrentCultureIgnoreCase)
            && !settings.ResultType.Equals("none", StringComparison.CurrentCultureIgnoreCase))
        {
            //Result type must be QUERY, SCALAR, or NONE if specified
            PrintUsage(Console.Out);
            return 1;
        }
        if (settings.UserId is not null && settings.Password is null)
        {
            settings.Password = GetPasswordFromConsole();
            if (string.IsNullOrWhiteSpace(settings.Password))
            {
                Console.Error.WriteLine("Password is required");
                return 2;
            }
        }

        using Runner runner = Runner.GetRunner(settings, extras);
        runner.Run();

        return 0;
    }

    private static void PrintUsage(TextWriter writer)
    {
        writer.WriteLine("Usage:");
        writer.WriteLine("\tRunSQL -server server -database database [-userId userId [-password password]?]? (-sql SQL | -in filepath) [-out filepath]? [-format CSV|JSON]? [-resultType QUERY|SCALAR|NONE]?");
    }

    public static string GetPasswordFromConsole()
    {
        Console.Write("Password: ");
        ConsoleKey key;
        Stack<char> inputs = new();
        do
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            key = keyInfo.Key;
            if (key == ConsoleKey.Backspace && inputs.Any())
            {
                _ = inputs.Pop(); //discard
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                inputs.Push(keyInfo.KeyChar);
            }
        } while (key != ConsoleKey.Enter);
        Console.WriteLine();
        return new string(inputs.Reverse().ToArray());
    }

}