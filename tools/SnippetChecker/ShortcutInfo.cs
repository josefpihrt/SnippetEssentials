// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Pihrtsoft.Snippets
{
    public class ShortcutInfo
    {
        public ShortcutInfo(string shortcut, IEnumerable<Snippet> snippets)
        {
            Shortcut = shortcut;
            Snippets = snippets;
        }

        public string Shortcut { get; }
        public IEnumerable<Snippet> Snippets { get; }
    }
}
