using System;
using System.Linq;
using System.Reflection;

public class Settings
{
    private class SettingsAttribute(string shortName, string longName, bool isMandatory = false) : Attribute
    {
        public string ShortName { get; set; } = shortName;
        public string LongName { get; set; } = longName;
        public bool IsMandatory { get; set; } = isMandatory;
    }

    [Settings("d", "dir", true)] 
    public string Directory { get; set; }

    [Settings("s", "sourceEncoding", true)]
    public string SourceEncoding { get; set; }

    [Settings("t", "targetEncoding", true)]
    public string TargetEncoding { get; set; }

    [Settings("p", "searchPattern", true)] 
    public string SearchPattern { get; set; }

    [Settings("r", "recursive")] 
    public bool Recursive { get; set; } = false;

    [Settings("e", "prefix")] 
    public string Prefix { get; set; }

    [Settings("f", "postfix")] 
    public string Postfix { get; set; }

    public static Settings Parse(string[] args)
    {
        var settings = new Settings();

        var properties = typeof(Settings)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => new { Property = p, Attribute = p.GetCustomAttribute<SettingsAttribute>() })
            .Where(x => x.Attribute != null)
            .ToArray();

        var argList = args.ToArray();

        for (var i = 0; i < argList.Length; i++)
        {
            var arg = argList[i];

            if (!arg.StartsWith("-"))
                continue;

            var isLong = arg.StartsWith("--");
            var key = isLong ? arg[2..] : arg[1..];
            var match = properties.FirstOrDefault(p => isLong ? p.Attribute.LongName == key : p.Attribute.ShortName == key);

            if (match == null)
                continue;

            var prop = match.Property;

            if (prop.PropertyType == typeof(bool))
            {
                var value = true;
                if (i + 1 < argList.Length && !argList[i + 1].StartsWith("-"))
                {
                    value = bool.Parse(argList[i + 1]);
                    i++;
                }

                prop.SetValue(settings, value);
            }
            else
            {
                if (i + 1 < argList.Length && !argList[i + 1].StartsWith("-"))
                {
                    var value = argList[i + 1];
                    prop.SetValue(settings, value);
                    i++;
                }
            }
        }

        return settings;
    }

    public void Validate()
    {
        var properties = typeof(Settings)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => new { Property = p, Attribute = p.GetCustomAttribute<SettingsAttribute>() })
            .Where(x => x.Attribute is { IsMandatory: true } && x.Property.GetValue(this) == null)
            .ToArray();
        
        foreach (var prop in properties)
        {
            Console.Error.WriteLine($"{prop.Property.Name} is not set (pass a value with --{prop.Attribute.LongName} or -{prop.Attribute.ShortName})");
        }

        if (properties.Length > 0)
            throw new Exception("Invalid settings");
    }
}