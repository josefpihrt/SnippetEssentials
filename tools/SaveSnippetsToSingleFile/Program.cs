using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pihrtsoft.Snippets;

namespace Pihrtsoft.Snippets
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Specify snippet directory:");

            string dirPath = Console.ReadLine();

            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine($"Directory not found: {dirPath}");
                Console.ReadKey();
                return;
            }

            var snippets = new List<Snippet>(LoadSnippets(dirPath).OrderBy(f => f.FilePath));

            var snippetFile = new SnippetFile(Path.Combine(dirPath, "snippets.xml"));

            if (File.Exists(snippetFile.FullName))
            {
                Console.WriteLine($"File already exists: {snippetFile.FullName}");
                Console.ReadKey();
                return;
            }

            foreach (Snippet snippet in snippets)
            {
                string category = Path.GetDirectoryName(snippet.FilePath)
                    .Replace(dirPath, string.Empty)
                    .TrimStart(Path.DirectorySeparatorChar)
                    .Replace(Path.DirectorySeparatorChar, '.');

                snippet.Keywords.Add("Category:" + category);
                snippet.Keywords.Add("FullyQualifiedName:" + category + "." + Path.GetFileNameWithoutExtension(snippet.FilePath));

                snippetFile.Snippets.Add(snippet);

                string relativePath = snippet.FilePath.Replace(dirPath, string.Empty);
                Console.WriteLine(relativePath);
            }

            var saveSettings = new SaveSettings();

            SnippetSerializer.Serialize(snippetFile, saveSettings);

            Console.WriteLine("FINISHED");
            Console.ReadKey();
        }

        private static IEnumerable<Snippet> LoadSnippets(string dirPath)
        {
            foreach (string dirPath2 in Directory.EnumerateDirectories(dirPath, "*", SearchOption.TopDirectoryOnly))
            {
                if (Path.GetFileName(dirPath2).StartsWith("_"))
                    continue;

                foreach (SnippetFile snippetFile in SnippetSerializer.DeserializeFiles(dirPath2, SearchOption.AllDirectories))
                {
                    foreach (Snippet snippet in snippetFile.Snippets)
                        yield return snippet;
                }
            }
        }
    }
}
