// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pihrtsoft.Snippets.Comparers;
using Pihrtsoft.Snippets.Validations;

namespace Pihrtsoft.Snippets
{
    public class SnippetChecker
    {
        private static readonly SnippetDeepEqualityComparer _snippetEqualityComparer = new SnippetDeepEqualityComparer();
        private static readonly StringComparer _ordinalStringComparer = StringComparer.Ordinal;

        private static IEnumerable<Snippet> DeserializeSnippets(IEnumerable<string> dirPaths)
        {
            foreach (string dirPath in dirPaths)
            {
                foreach (Snippet snippet in SnippetSerializer.Deserialize(dirPath, SearchOption.AllDirectories))
                    yield return snippet;
            }
        }

        public static IEnumerable<ShortcutInfo> FindDuplicateShortcuts(List<string> dirPaths, string allowDuplicateKeyword)
        {
            IEnumerable<Snippet> snippets = DeserializeSnippets(dirPaths);

            foreach (var grouping in snippets
                .SelectMany(snippet => snippet.Shortcuts()
                    .Select(shortcut => new { Shortcut = shortcut, Snippet = snippet }))
                .GroupBy(f => f.Shortcut, _ordinalStringComparer))
            {
                if (!string.IsNullOrEmpty(grouping.Key))
                {
                    using (var en = grouping.GetEnumerator())
                    {
                        if (en.MoveNext() && en.MoveNext())
                        {
                            if (allowDuplicateKeyword == null
                                || grouping.Any(f => !f.Snippet.Keywords.Contains(allowDuplicateKeyword)))
                            {
                                yield return new ShortcutInfo(grouping.Key, grouping.Select(f => f.Snippet));
                            }
                        }
                    }
                }
            }
        }

        public static IEnumerable<SnippetValidationResult> Validate(List<Snippet> snippets)
        {
            var validator = new CustomSnippetValidator();

            foreach (Snippet snippet in snippets)
            {
                foreach (SnippetValidationResult result in validator.Validate(snippet))
                    yield return result;
            }
        }

        public static Snippet CloneAndSortCollections(Snippet snippet)
        {
            var clone = (Snippet)snippet.Clone();

            clone.Literals.Sort();
            clone.Keywords.Sort();
            clone.Namespaces.Sort();
            clone.AlternativeShortcuts.Sort();

            return clone;
        }

        public static Snippet GetChangedSnippetOrDefault(Snippet snippet)
        {
            var snippet2 = CloneAndSortCollections(snippet);

            if (!_snippetEqualityComparer.Equals(snippet, snippet2))
                return snippet2;

            return null;
        }
    }
}
