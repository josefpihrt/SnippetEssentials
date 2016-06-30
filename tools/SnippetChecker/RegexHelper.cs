// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using static Pihrtsoft.Text.RegularExpressions.Linq.Patterns;

namespace Pihrtsoft.Snippets
{
    public static class RegexHelper
    {
        public static readonly Regex TypeName =
            NotAssertBack(BeginLine().WhileWhiteSpaceExceptNewLine().Slash(3).CrawlNative())
                .Any("string", "int", "bool", "long", "object")
                .Dot()
                .ToRegex();

        public static readonly Regex TrimEnd =
            Spaces()
                .Assert(NewLine(), EndInput())
                .ToRegex();

        public static readonly Regex InvalidLeadingSpaces =
            BeginLine()
                .OneMany(Space(4))
                .Count(1, 3, Space())
                .Any(NotSpace(), EndInput())
                .WhileNotNewLineChar()
                .ToRegex();

        public static readonly Regex CSharpComment =
            BeginLine().WhileWhiteSpaceExceptNewLine().Slash(3).ToRegex();
    }
}
