namespace Domain.Interfaces;
public interface IFileReader
{
    IEnumerable<string> ReadLines(string path);
    bool FileExists(string path);
}
