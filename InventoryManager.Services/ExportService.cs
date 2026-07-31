using System.Reflection;
using System.Text;
using InventoryManager.Core.Interfaces;

namespace InventoryManager.Services;

public class ExportService : IExportService
{
    public string ToCsv<T>(IEnumerable<T> rows)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var sb = new StringBuilder();

        sb.AppendLine(string.Join(",", properties.Select(p => EscapeCsvField(p.Name))));

        foreach (var row in rows)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(row);
                return EscapeCsvField(value?.ToString() ?? string.Empty);
            });

            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}