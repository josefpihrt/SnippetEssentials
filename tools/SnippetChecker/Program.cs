// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Pihrtsoft.Snippets;
using Pihrtsoft.Snippets.Comparers;
using Pihrtsoft.Snippets.Validations;

namespace SnippetChecker
{
    internal class Program
    {
        private static readonly SnippetDeepEqualityComparer _snippetEqualityComparer = new SnippetDeepEqualityComparer();

        static void Main(string[] args)
        {
#if DEBUG
            string mainDirPath = @"..\..\..\..\SnippetEssentials\SnippetEssentials.CSharp";
#else
            string mainDirPath = @"..\SnippetEssentials\SnippetEssentials.CSharp";
#endif


            var dirPaths = new List<string>();
            dirPaths.Add(mainDirPath);
            dirPaths.Add(@"D:\SkyDrive\programování\Snippets\Snippets\_CSharp");
            dirPaths.Add(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC#\Snippets");

            foreach (var dirPath in dirPaths)
            {
                if (!Directory.Exists(dirPath))
                {
                    Console.WriteLine($"directory not found '{dirPath}'");
                    Console.ReadKey();
                    return;
                }
            }

            var snippets = new List<Snippet>(SnippetSerializer.DeserializeFiles(mainDirPath, SearchOption.AllDirectories).SelectMany(f => f.Snippets));

            Console.WriteLine($"number of snippets: {snippets.Count}");

            Validate(snippets);
            FindDuplicates(dirPaths);

            var settings = new SaveSettings();
            settings.Comment = "Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0.";

            foreach (Snippet snippet in ProcessSnippets(snippets))
            {
                Console.WriteLine($"saving modified snippet '{snippet.Title}'");
                Save(mainDirPath, snippet, settings);
            }

            Console.WriteLine("*** FINISHED ***");
            Console.ReadKey();
        }

        private static void Validate(List<Snippet> snippets)
        {
            var validator = new CustomSnippetValidator();

            foreach (Snippet snippet in snippets)
            {
                foreach (SnippetValidationResult result in validator.Validate(snippet))
                {
                    Console.WriteLine($"{result.Importance} {result.Code} {result.Description} {result.Snippet.Title} {Path.GetFileName(result.Snippet.FilePath)}");
                    Debug.WriteLine($"{result.Importance} {result.Code} {result.Description} {result.Snippet.Title} {Path.GetFileName(result.Snippet.FilePath)}");
                }
            }
        }

        private static void FindDuplicates(List<string> dirPaths)
        {
            List<Snippet> snippets = DeserializeSnippets(dirPaths).ToList();

            foreach (IGrouping<string, Snippet> grouping in FindDuplicateShortcuts(snippets))
            {
                if (grouping.All(f => f.Keywords.Contains("NonUniqueShortcut")))
                    continue;

                Console.WriteLine(grouping.Key);

                foreach (Snippet item in grouping)
                    Console.WriteLine($"  {item.Title}");

                Console.WriteLine();
            }
        }

        private static IEnumerable<Snippet> DeserializeSnippets(IEnumerable<string> dirPaths)
        {
            foreach (string dirPath in dirPaths)
            {
                foreach (SnippetFile snippetFile in SnippetSerializer.DeserializeFiles(dirPath, SearchOption.AllDirectories))
                {
                    foreach (Snippet snippet in snippetFile.Snippets)
                        yield return snippet;
                }
            }
        }

        private static void Save(string dirPath, Snippet snippet, SaveSettings settings)
        {
            string filePath = snippet.FilePath + ".modified";
            snippet.Save(filePath, settings);
        }

        private static IEnumerable<Snippet> ProcessSnippets(IList<Snippet> snippets)
        {
            foreach (Snippet snippet in snippets)
            {
                var s = (Snippet)snippet.Clone();

                s.Literals.Sort();
                s.Keywords.Sort();
                s.Namespaces.Sort();

                if (!_snippetEqualityComparer.Equals(snippet, s))
                    yield return s;
            }
        }

        private static readonly StringComparer _stringComparer = StringComparer.CurrentCultureIgnoreCase;

        public static IEnumerable<IGrouping<string, Snippet>> FindDuplicateShortcuts(IEnumerable<Snippet> snippets)
        {
            if (snippets == null)
                throw new ArgumentNullException(nameof(snippets));

            foreach (IGrouping<string, Snippet> grouping in snippets
                .Where(f => f.Shortcut.Length > 0)
                .GroupBy(f => f.Shortcut, _stringComparer))
            {
                if (grouping.Count() > 1)
                    yield return grouping;
            }
        }
    }
}
