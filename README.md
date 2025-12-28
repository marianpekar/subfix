# Subfix

A simple command line tool I implemented when I needed to re-encode a large number of old subtitles from Windows-1250 to UTF-8:

```txt
-d <path to a directory> -p *.srt -s Windows-1250 -t UTF-8 --recursive
```

This commands cause the original files to be overwritten. To prevent that, you can add a prefix, postfix, or both:

```txt
-d <path to a directory> -p *.srt -s Windows-1250 -t UTF-8 --recursive -e <some prefix> -f <some postfix>
```

All parameters have long (--) and short (-) variants. See the attributes in the `Settings` class.

To specify source and target encodings (-s and -t parameters), you can use either a name such as utf-32 or windows-1250, or a codepage such as 1200 or 1250. See
https://learn.microsoft.com/en-us/dotnet/api/system.text.encoding.getencoding?view=net-9.0