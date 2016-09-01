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

            foreach (IGrouping<string, Snippet> grouping in SnippetChecker.FindDuplicateShortcuts(dirPaths, "NonUniqueShortcut"))
            {
                Console.WriteLine();
                Console.WriteLine($"shortcut duplicate: {grouping.Key}");

                foreach (Snippet item in grouping)
                    Console.WriteLine($"  {item.FilePath}");
            };

            var settings = new SaveSettings() { Comment = "Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0." };

            foreach (Snippet snippet in SnippetChecker.GetSnippetsToSave(snippets))
            {
                Console.WriteLine();
                Console.WriteLine($"saving modified snippet \"{snippet.Title}\"");

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

            Assert(false);
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
