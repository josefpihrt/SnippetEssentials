// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pihrtsoft.Snippets.Validations;

namespace Pihrtsoft.Snippets
{
    internal class Program
    {
        private const string XmlComment = "Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0.";
        private const string NonUniqueShortcutKeyword = "Meta-NonUniqueShortcut";

        static void Main(string[] args)
        {
            var dirPaths = new List<string>();

            if (args?.Length > 0)
            {
                dirPaths.AddRange(args);
            }
            else
            {
                dirPaths.Add(Environment.CurrentDirectory);
            }

            Console.WriteLine();
            Console.WriteLine("directories:");

            for (int i = 0; i < dirPaths.Count; i++)
            {
                Console.WriteLine($"  {dirPaths[i]}");

                if (!Assert(Directory.Exists(dirPaths[i]), "directory not found"))
                    return;
            }

            var snippets = new List<Snippet>(SnippetSerializer.DeserializeFiles(dirPaths[0], SearchOption.AllDirectories).SelectMany(f => f.Snippets));

            Console.WriteLine();
            Console.WriteLine($"number of snippets: {snippets.Count}");

            foreach (SnippetValidationResult result in SnippetChecker.Validate(snippets))
            {
                Console.WriteLine();
                Console.WriteLine($"{result.Importance}: \"{result.Description}\" in \"{result.Snippet.FilePath}\"");
            }

            foreach (IGrouping<string, Snippet> snippet in snippets
                .Where(f => f.Keywords.Contains(NonUniqueShortcutKeyword))
                .GroupBy(f => f.Shortcut)
                .Where(f => f.Count() == 1))
            {
                Console.WriteLine();
                Console.WriteLine($"unused {NonUniqueShortcutKeyword} in \"{snippet.First().FilePath}\"");
            }

            foreach (ShortcutInfo shortcutInfo in SnippetChecker.FindDuplicateShortcuts(dirPaths, NonUniqueShortcutKeyword))
            {
                Console.WriteLine();
                Console.WriteLine($"shortcut duplicate: {shortcutInfo.Shortcut}");

                foreach (Snippet item in shortcutInfo.Snippets)
                    Console.WriteLine($"  {item.FilePath}");
            };

            SaveSnippets(snippets);

            Assert(false);
        }

        private static void SaveSnippets(List<Snippet> snippets)
        {
            var settings = new SaveSettings()
            {
                Comment = XmlComment,
            };

            foreach (Snippet snippet in snippets
                .Select(f => SnippetChecker.GetChangedSnippetOrDefault(f))
                .Where(f => f != null))
            {
                Console.WriteLine();
                Console.WriteLine($"saving snippet \"{snippet.Title}\"");

                try
                {
                    snippet.Save(snippet.FilePath + ".modified", settings);

                    Console.WriteLine("saved");
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void SaveSnippets2(List<Snippet> snippets)
        {
            var settings = new SaveSettings()
            {
                Comment = XmlComment,
                OmitCodeSnippetsElement = true,
                IndentChars = "  ",
                OmitXmlDeclaration = true
            };

            foreach (Snippet snippet in snippets
                    .Select(f => SnippetChecker.CloneAndSortCollections(f)))
            {
                Console.WriteLine();
                Console.WriteLine($"saving snippet \"{snippet.Title}\"");

                try
                {
                    using (var fs = new FileStream(snippet.FilePath, FileMode.Create))
                        snippet.Save(fs, settings);

                    Console.WriteLine("saved");
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static bool Assert(bool condition, string message = null)
        {
            if (!condition)
            {
                if (!string.IsNullOrEmpty(message))
                    Console.WriteLine(message);

                Console.WriteLine();
                Console.WriteLine("*** END ***");
                Console.ReadKey();
            }

            return condition;
        }
    }
}
