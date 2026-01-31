using System.Text;

namespace Tests.Infrastructure;

internal class TempCsvFile :  IDisposable
{
    public string Path { get; }

    public TempCsvFile(params  string[] lines)
    {
        string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
        File.WriteAllLines(tempPath, lines);
        Path = tempPath;
    }

    public void Dispose()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
        }
    }
}