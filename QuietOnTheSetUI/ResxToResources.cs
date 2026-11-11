using System;
using System.Collections;
using System.IO;
using System.Resources;

internal static class ResxToResources
{
    private static int Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("Usage: ResxToResources <input.resx> <output.resources>");
            return 1;
        }

        string resx = args[0];
        string output = args[1];
        using (var reader = new ResXResourceReader(resx))
        using (var writer = new ResourceWriter(output))
        {
            reader.BasePath = Path.GetDirectoryName(Path.GetFullPath(resx));
            foreach (DictionaryEntry entry in reader)
            {
                writer.AddResource((string)entry.Key, entry.Value);
            }
        }
        return 0;
    }
}
