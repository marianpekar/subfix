using System;
using System.IO;

namespace Subfix;

public class Logger : IDisposable
{
    private readonly Action<string> _writer;
    private readonly Action<string> _writerError;
    private readonly TextWriter _textWriter;

    public Logger(string logFile)
    {
        if (string.IsNullOrEmpty(logFile))
        {
            _writer = Console.WriteLine;
            _writerError = Console.Error.WriteLine;
        }
        else
        {
            _writer = _writerError = WriteToLogFile;
            _textWriter = TextWriter.Synchronized(File.AppendText(logFile));
        }
    }

    public void Write(string value)
    {
        _writer(Timestamp(value));
    }

    public void WriteError(string value)
    {
        _writerError(Timestamp(value));
    }

    private void WriteToLogFile(string value)
    {
        _textWriter.WriteLine(value);
    }
    
    private static string Timestamp(string value)
    {
        return $"[{DateTime.Now:dd/MM/yyyy hh:mm:ss}] {value}";
    }

    public void Dispose()
    {
        _textWriter?.Dispose();
    }
}