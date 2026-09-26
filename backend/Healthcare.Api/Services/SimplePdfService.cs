using System.Text;

namespace Healthcare.Api.Services;

public class SimplePdfService
{
    public byte[] Create(string title, IEnumerable<string> lines)
    {
        var safeLines = new[] { title }.Concat(lines).Select(Escape).ToList();
        var content = new StringBuilder();
        content.AppendLine("BT /F1 12 Tf 50 780 Td");
        for (int i = 0; i < safeLines.Count; i++)
        {
            if (i > 0) content.Append("0 -18 Td ");
            content.Append($"({safeLines[i]}) Tj ");
        }
        content.AppendLine("ET");
        var stream = Encoding.ASCII.GetBytes(content.ToString());

        var objects = new List<string>
        {
            "1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj\n",
            "2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj\n",
            "3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >> endobj\n",
            $"4 0 obj << /Length {stream.Length} >> stream\n{content}endstream\nendobj\n",
            "5 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj\n"
        };

        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.ASCII, leaveOpen: true);
        writer.Write("%PDF-1.4\n");
        writer.Flush();
        var offsets = new List<long> { 0 };
        foreach (var obj in objects)
        {
            offsets.Add(ms.Position);
            writer.Write(obj);
            writer.Flush();
        }
        var xref = ms.Position;
        writer.Write($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1)) writer.Write($"{offset:0000000000} 00000 n \n");
        writer.Write($"trailer << /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
        writer.Flush();
        return ms.ToArray();
    }

    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
}
