// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace Pihrtsoft.Snippets
{
    public static class RegexHelper
    {
        public static readonly Regex PredefinedTypeName = new Regex(
            @"
            (?<!
                (?m:
                    ^
                )
                [\s-[\r\n]]*
                /{3}
                .*?
            )
            (?:
                string
            |
                int
            |
                bool
            |
                long
            |
                object
            )
            \s*
            \.
            ",
            RegexOptions.IgnorePatternWhitespace);

        //public static readonly Regex PredefinedTypeName =
        //    NotAssertBack(BeginLine().WhileWhiteSpaceExceptNewLine().Slash(3).CrawlNative())
        //        .Any("string", "int", "bool", "long", "object")
        //        .Dot()
        //        .ToRegex();

        public static readonly Regex TrimEnd = new Regex(
            @"
            \ +
            (?=
                (?:
                    \r?
                    \n
                )
            |
                \z
            )
            "
            , RegexOptions.IgnorePatternWhitespace);

        //public static readonly Regex TrimEnd =
        //    Spaces()
        //        .Assert(NewLine(), EndInput())
        //        .ToRegex();

        public static readonly Regex InvalidLeadingSpaces = new Regex(
            @"
            (?m:
                ^
            )
            (?:
                \ {2}
            )+
            \ {1}
            (?:
                [^ ]
            |
                \z
            )
            [^\r\n]*
            "
            , RegexOptions.IgnorePatternWhitespace);

        //public static readonly Regex InvalidLeadingSpaces =
        //    BeginLine()
        //        .OneMany(Space(4))
        //        .Count(1, 3, Space())
        //        .Any(NotSpace(), EndInput())
        //        .WhileNotNewLineChar()
        //        .ToRegex();
    }
}
