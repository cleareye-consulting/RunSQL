using System.Data;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

public class QueryRunner : Runner
{
    public QueryRunner(Arguments settings, IDictionary<string, string> extras) : base(settings, extras) { }

    public override void Run()
    {
        Connect();
        using SqlCommand command = new(GetSql(), connection);
        using DataTable dt = new();
        using SqlDataAdapter da = new(command);
        da.Fill(dt);
        if (settings.Out is not null)
        {
            if ((settings.Format ?? "CSV").Equals("json", StringComparison.CurrentCultureIgnoreCase))
            {
                WriteJsonFile(dt, settings.Out);
            }
            else
            {
                WriteCsvFile(dt, settings.Out);
            }
        }
        else
        {
            if ((settings.Format ?? "").Equals("json", StringComparison.CurrentCultureIgnoreCase))
            {
                WriteJsonOut(dt, Console.Out);
            }
            else
            {
                WriteAsciiTableOut(dt, Console.Out);
            }
        }
    }

    const string q = "\"";
    const string qcq = "\",\"";

    private static void WriteCsvFile(DataTable dt, string path)
    {
        using StreamWriter writer = new(path);
        writer.WriteLine($"{q}{string.Join(qcq, dt.Columns.Cast<DataColumn>().Select(dc => dc.ColumnName))}{q}");
        foreach (DataRow dr in dt.Rows)
        {
            writer.WriteLine($"{q}{string.Join(qcq, dt.Columns.Cast<DataColumn>().Select(col => dr[col.ColumnName]))}{q}");
        }
        writer.Flush();
    }

    private static void WriteJsonFile(DataTable dt, string path)
    {
        File.WriteAllText(path, JsonConvert.SerializeObject(dt));
    }

    private static void WriteJsonOut(DataTable dt, TextWriter writer)
    {
        writer.WriteLine(JsonConvert.SerializeObject(dt));
    }

    private static void WriteAsciiTableOut(DataTable dt, TextWriter writer)
    {
        int[] maxWidths = Enumerable.Repeat(int.MinValue, dt.Columns.Count).ToArray();
        foreach (DataRow dr in dt.Rows)
        {
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string value = dr[i]?.ToString() ?? "";
                if (value.Length > maxWidths[i])
                {
                    maxWidths[i] = value.Length;
                }
            }
        }
        for (int i = 0; i < dt.Columns.Count; i++)
        {
            int columnNameLength = dt.Columns[i].ColumnName.Length;
            if (columnNameLength > maxWidths[i])
            {
                maxWidths[i] = columnNameLength;
            }
        }
        int totalDataWidth = maxWidths.Sum();
        int fenceposts = dt.Columns.Count + 1;
        int totalHorizontalLength = totalDataWidth + fenceposts;
        string horizontal = new string('-', totalHorizontalLength);
        writer.WriteLine(horizontal);
        for (int i = 0; i < dt.Columns.Count; i++)
        {
            if (i == 0)
            {
                writer.Write('|');
            }
            var (left, right) = GetPaddingNeeded(dt.Columns[i].ColumnName.Length, maxWidths[i]);
            writer.Write(new string(' ', left));
            writer.Write(dt.Columns[i].ColumnName);
            writer.Write(new string(' ', right));
            writer.Write('|');
        }
        writer.WriteLine();
        writer.WriteLine(horizontal);
        foreach (DataRow dr in dt.Rows)
        {
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i == 0)
                {
                    writer.Write('|');
                }
                string value = dr[i]?.ToString() ?? "";
                var (left, right) = GetPaddingNeeded(value.Length, maxWidths[i]);
                writer.Write(new string(' ', left));
                writer.Write(value);
                writer.Write(new string(' ', right));
                writer.Write('|');
            }
            writer.WriteLine();
            writer.WriteLine(horizontal);
        }
        writer.Flush();
        writer.Close();
    }

    private static (int left, int right) GetPaddingNeeded(int length, int maxLength)
    {
        int difference = maxLength - length;
        if (difference == 0)
        {
            return (0, 0);
        }
        if (difference % 2 == 0)
        {
            return (difference / 2, difference / 2);
        }
        return ((difference - 1) / 2 + 1, (difference - 1) / 2);
    }

}