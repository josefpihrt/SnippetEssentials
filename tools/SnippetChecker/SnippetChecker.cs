// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        private static IEnumerable<Snippet> DeserializeSnippets(IEnumerable<string> dirPaths)
        {
            foreach (string dirPath in dirPaths)
            {
                foreach (Snippet snippet in SnippetSerializer.Deserialize(dirPath, SearchOption.AllDirectories))
                    yield return snippet;
            }
        }

        public static IEnumerable<IGrouping<string, Snippet>> FindDuplicateShortcuts(List<string> dirPaths, string allowDuplicateKeyword)
        {
            IEnumerable<Snippet> snippets = DeserializeSnippets(dirPaths);

            foreach (IGrouping<string, Snippet> grouping in SnippetUtility.FindDuplicateShortcuts(snippets))
            {
                if (string.IsNullOrEmpty(grouping.Key))
                    continue;

                if (allowDuplicateKeyword != null
                    && grouping.All(f => f.Keywords.Contains(allowDuplicateKeyword)))
                {
                    continue;
                }

                yield return grouping;
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

        public static IEnumerable<Snippet> GetSnippetsToSave(IEnumerable<Snippet> snippets)
        {
            return snippets
                .Select(f => GetUpdatedSnippet(f))
                .Where(f => f != null);
        }

        private static Snippet GetUpdatedSnippet(Snippet snippet)
        {
            var snippet2 = (Snippet)snippet.Clone();

            snippet2.Literals.Sort();
            snippet2.Keywords.Sort();
            snippet2.Namespaces.Sort();

            if (!_snippetEqualityComparer.Equals(snippet, snippet2))
                return snippet2;

            return null;
        }
    }
}
