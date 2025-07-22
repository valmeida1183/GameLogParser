using Domain.Interfaces;

namespace Domain;
public class FileReader : IFileReader
{
    public IEnumerable<string> ReadLines(string path) => File.ReadLines(path);
    public bool FileExists(string path) => File.Exists(path);
}
