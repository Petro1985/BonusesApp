using System.Globalization;
using System.Text;
using BonusesApp.Core.Models.Bonuses;
using BonusesApp.Core.Services.Bonuses;

namespace BonusesApp.Server.Services;

/// <summary>
/// Reads the CSV produced by the bonuses table download.
/// Columns, in order: id, phone, name, comment, total, current, setting, last update.
/// The header is localized and the id column title is empty. Values are quoted and separated by ';'.
/// </summary>
public static class BonusesCsvReader
{
    private static readonly HashSet<string> PhoneHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "phonenumber", "phone", "номертелефона"
    };

    private static readonly HashSet<string> NameHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "name", "clientname", "имяклиента", "имя"
    };

    private static readonly HashSet<string> CommentHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "comment", "комментарий"
    };

    private static readonly HashSet<string> TotalHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "totalcounter", "всего", "общееколичество"
    };

    private static readonly HashSet<string> CurrentHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "currentcounter", "текущеезначение", "текущееколичество"
    };

    private static readonly HashSet<string> SettingHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "setting", "настройка"
    };

    public static List<BonusesImportItem> Read(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var text = reader.ReadToEnd();
        if (text.Length > 0 && text[0] == '\uFEFF')
        {
            text = text[1..];
        }

        var table = Parse(text);
        if (table.Count == 0)
        {
            throw new ArgumentException("Файл пуст.");
        }

        var (phone, name, comment, total, current, setting, dataStart) = ResolveColumns(table);
        var rows = new List<BonusesImportItem>();

        for (var index = dataStart; index < table.Count; index++)
        {
            var line = table[index];
            if (line.Cells.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var phoneNumber = Cell(line, phone);
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException($"Строка {line.Number}: не указан номер телефона.");
            }

            var commentText = Cell(line, comment);
            if (commentText == "null")
            {
                commentText = "";
            }

            rows.Add(new BonusesImportItem
            {
                PhoneNumber = phoneNumber,
                Name = Cell(line, name),
                Comment = string.IsNullOrEmpty(commentText) ? null : commentText,
                TotalCount = RequireInt(Cell(line, total), "Всего", line.Number),
                CurrentCount = RequireInt(Cell(line, current), "Текущее значение", line.Number),
                Setting = RequireInt(Cell(line, setting), "Настройка", line.Number)
            });
        }

        return rows;
    }

    private static (int Phone, int Name, int Comment, int Total, int Current, int Setting, int DataStart) ResolveColumns(
        List<CsvRow> table)
    {
        var header = table[0];
        var phone = IndexOf(header, PhoneHeaders);
        if (phone >= 0)
        {
            return (
                phone,
                IndexOf(header, NameHeaders),
                IndexOf(header, CommentHeaders),
                IndexOf(header, TotalHeaders),
                IndexOf(header, CurrentHeaders),
                IndexOf(header, SettingHeaders),
                1);
        }

        // Download writes an empty title for the id column, then the localized titles.
        if (header.Cells.Count >= 8 && !LooksLikePhone(header.Cells[1]))
        {
            return (1, 2, 3, 4, 5, 6, 1);
        }

        if (header.Cells.Count >= 8 && LooksLikePhone(header.Cells[1]))
        {
            return (1, 2, 3, 4, 5, 6, 0);
        }

        throw new ArgumentException("Не удалось прочитать файл. Загрузите CSV, скачанный с этой страницы.");
    }

    private static int IndexOf(CsvRow header, HashSet<string> names)
    {
        for (var i = 0; i < header.Cells.Count; i++)
        {
            if (names.Contains(HeaderKey(header.Cells[i])))
            {
                return i;
            }
        }

        return -1;
    }

    private static string HeaderKey(string header) =>
        header.Trim().Replace(" ", "", StringComparison.Ordinal).ToLowerInvariant();

    private static string Cell(CsvRow row, int index)
    {
        if (index < 0 || index >= row.Cells.Count)
        {
            return "";
        }

        return row.Cells[index];
    }

    private static int RequireInt(string raw, string field, int line)
    {
        if (!int.TryParse(raw.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            throw new ArgumentException($"Строка {line}: некорректное значение поля «{field}».");
        }

        return value;
    }

    private static bool LooksLikePhone(string value) =>
        BonusService.NormalizePhone(value).Length >= 10;

    private static List<CsvRow> Parse(string text)
    {
        var rows = new List<CsvRow>();
        var row = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var line = 1;

        for (var i = 0; i < text.Length; i++)
        {
            var current = text[i];
            if (inQuotes)
            {
                if (current == '\\' && i + 1 < text.Length && text[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else if (current == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else if (current != '\r')
                {
                    field.Append(current);
                }
            }
            else if (current == '"')
            {
                inQuotes = true;
            }
            else if (current == ';')
            {
                row.Add(field.ToString());
                field.Clear();
            }
            else if (current == '\n')
            {
                row.Add(field.ToString());
                field.Clear();
                rows.Add(new CsvRow(line, row));
                row = [];
                line++;
            }
            else if (current != '\r')
            {
                field.Append(current);
            }
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(new CsvRow(line, row));
        }

        return rows;
    }

    private sealed record CsvRow(int Number, List<string> Cells);
}
