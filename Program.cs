using System.IO;
using System.Text;
using System.Threading.Tasks;
using Subfix;

static class Program
{
    static void Main(string[] args)
    {
        var settings = Settings.Parse(args);
        using var logger = new Logger(settings.LogFile);
        settings.Validate(logger);

        if (!Directory.Exists(settings.Directory))
        {
            string message = $"'{settings.Directory}' directory not found";
            logger.WriteError(message);
            throw new DirectoryNotFoundException(message);
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var sourceEncoding = GetEncoding(settings.SourceEncoding);
        var targetEncoding = GetEncoding(settings.TargetEncoding);

        var paths = Directory.GetFiles(settings.Directory, settings.SearchPattern, settings.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        var changeFilename = !string.IsNullOrEmpty(settings.Prefix) || !string.IsNullOrEmpty(settings.Postfix);

        if (paths.Length == 0)
        {
            logger.Write("Nothing to process");
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
            if (!settings.Silent)
            {
                logger.Write($"'{path}' saved");
            }
        });
        
        logger.Write($"All files re-encoded from '{sourceEncoding.EncodingName}' to '{targetEncoding.EncodingName}'");
        return;

        Encoding GetEncoding(string arg)
        {
            return int.TryParse(arg, out int codepage) ? Encoding.GetEncoding(codepage) : Encoding.GetEncoding(arg);
        }
    }
}