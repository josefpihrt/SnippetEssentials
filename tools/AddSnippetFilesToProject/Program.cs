using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Pihrtsoft.Snippets
{
    internal class Program
    {
        private const string CsProjFileName = "SnippetEssentials.csproj";

        private static void Main(string[] args)
        {
#if DEBUG
            string projectDirPath = @"D:\Documents\Visual Studio 2015\Projects\SnippetEssentials\SnippetEssentials";
#else
            string projectDirPath = Environment.CurrentDirectory;
#endif

            string filePath = Path.Combine(projectDirPath, CsProjFileName);

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"file '{filePath}' not found.");
                Console.ReadKey();
                return;
            }

            XDocument doc = XDocument.Load(filePath);

            XElement itemGroup = FindItemGroupElement(doc);

            XElement[] nodesToRemove = GetSnippetFileReferenceToRemove(itemGroup).ToArray();

            for (int i = 0; i < nodesToRemove.Length; i++)
                nodesToRemove[i].Remove();

            string directoryPath = Path.Combine(projectDirPath, "SnippetEssentials.CSharp");
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"directory '{directoryPath}' not found.");
                Console.ReadKey();
                return;
            }

            int addedCount = AddSnippetFileReferences(itemGroup, directoryPath);

            Console.WriteLine();
            Console.WriteLine($"{nodesToRemove.Length} snippet file references removed from {CsProjFileName}");
            Console.WriteLine($"{addedCount} snippet files added to {CsProjFileName}");

            doc.Save(filePath);
            Console.WriteLine($"{CsProjFileName} saved");
            Console.WriteLine("FINISHED");

            Console.ReadKey();
        }

        private static XElement FindItemGroupElement(XDocument doc)
        {
            foreach (XElement element in doc
                .Descendants()
                .Where(f => f.Name.LocalName == "ItemGroup"))
            {
                if (element
                    .Elements()
                    .Any(f => f.Name.LocalName == "Content"))
                {
                    if (element
                        .Attributes()
                        .Any(f => f.Name.LocalName == "Include" && f.Value == "regedit.pkgdef"))
                    {
                        return element;
                    }

                    return element;
                }
            }

            return null;
        }

        private static IEnumerable<XElement> GetSnippetFileReferenceToRemove(XElement parent)
        {
            foreach (XElement element in parent.Elements())
            {
                if (element.Name.LocalName == "Content")
                {
                    Debug.WriteLine(element.Name);
                    if (element
                        .Attributes()
                        .Any(f => f.Name.LocalName == "Include" && f.Value.EndsWith(".snippet")))
                    {
                        yield return element;
                    }
                }
            }
        }

        private static int AddSnippetFileReferences(XElement element, string projectDirectoryPath)
        {
            int cnt = 0;

            foreach (string filePath in Directory.EnumerateFiles(
                projectDirectoryPath,
                "*.snippet",
                SearchOption.AllDirectories))
            {
                string relativePath = filePath
                    .Replace(Path.GetDirectoryName(projectDirectoryPath), string.Empty)
                    .TrimStart(Path.DirectorySeparatorChar);

                XNamespace ns = element.Name.Namespace;

                element.Add(new XElement(ns + "Content",
                    new XAttribute("Include", relativePath),
                    new XElement(ns + "IncludeInVSIX", "true")));

                Console.WriteLine($"{relativePath} add to {CsProjFileName}");
                cnt++;
            }

            return cnt;
        }
    }
}
