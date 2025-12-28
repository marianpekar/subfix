using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

static class Program
{
    static void Main(string[] args)
    {
        var settings = Settings.Parse(args);
        settings.Validate();

        if (!Directory.Exists(settings.Directory))
        {
            throw new DirectoryNotFoundException($"'{settings.Directory}' directory not found");
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var sourceEncoding = GetEncoding(settings.SourceEncoding);
        var targetEncoding = GetEncoding(settings.TargetEncoding);

        var paths = Directory.GetFiles(settings.Directory, settings.SearchPattern, settings.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        var changeFilename = !string.IsNullOrEmpty(settings.Prefix) || !string.IsNullOrEmpty(settings.Postfix);

        if (paths.Length == 0)
        {
            Console.WriteLine("Nothing to process");
            return;
        }

        Parallel.ForEach(paths, path =>
        {
            var bytes = File.ReadAllBytes(path);
            var content = sourceEncoding.GetString(bytes);

            if (changeFilename)
            {
                var newFilename = settings.Prefix + Path.GetFileNameWithoutExtension(path) + settings.Postfix + Path.GetExtension(path);
                var currentDir = Path.GetDirectoryName(path);
                path = Path.Combine(currentDir, newFilename);
            }

            File.WriteAllText(path, content, targetEncoding);
            Console.WriteLine($"'{path}' saved");
        });

        Console.WriteLine($"All files re-encoded from '{sourceEncoding.EncodingName}' to '{targetEncoding.EncodingName}'\nTask finished successfully");
        return;

        Encoding GetEncoding(string arg)
        {
            return int.TryParse(arg, out int codepage) ? Encoding.GetEncoding(codepage) : Encoding.GetEncoding(arg);
        }
    }
}