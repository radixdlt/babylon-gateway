using System.Text;
using System.Text.Json;

namespace Common.Extensions;

public static class StringExtensions
{
    public static string Truncate(this string str, int maxLength)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        return str.Length <= maxLength ? str : str[..maxLength];
    }

    public static byte[] ConvertFromHex(this string str)
    {
        return Convert.FromHexString(str);
    }

    public static string ToSnakeCase(this string str)
    {
        return NewtonsoftStringUtils.ToSeparatedCase(str, '_');
    }
}

/// <summary>
/// These have been copied, with minor amendments, from https://github.com/JamesNK/Newtonsoft.Json/blob/42139ea6cd8d500790bf27c9afc69fd66eab60ad/Src/Newtonsoft.Json/Utilities/StringUtils.cs
/// With the notice copied below.
/// </summary>
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
public class NewtonsoftStringUtils
{
    private enum SeparatedCaseState
    {
        Start,
        Lower,
        Upper,
        NewWord,
    }

    public static string ToSeparatedCase(string s, char separator)
    {
        if (string.IsNullOrEmpty(s))
        {
            return s;
        }

        StringBuilder sb = new StringBuilder();
        SeparatedCaseState state = SeparatedCaseState.Start;

        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == ' ')
            {
                if (state != SeparatedCaseState.Start)
                {
                    state = SeparatedCaseState.NewWord;
                }
            }
            else if (char.IsUpper(s[i]))
            {
                switch (state)
                {
                    case SeparatedCaseState.Upper:
                        bool hasNext = i + 1 < s.Length;
                        if (i > 0 && hasNext)
                        {
                            char nextChar = s[i + 1];
                            if (!char.IsUpper(nextChar) && nextChar != separator)
                            {
                                sb.Append(separator);
                            }
                        }
                        break;
                    case SeparatedCaseState.Lower:
                    case SeparatedCaseState.NewWord:
                        sb.Append(separator);
                        break;
                }

                sb.Append(char.ToLowerInvariant(s[i]));

                state = SeparatedCaseState.Upper;
            }
            else if (s[i] == separator)
            {
                sb.Append(separator);
                state = SeparatedCaseState.Start;
            }
            else
            {
                if (state == SeparatedCaseState.NewWord)
                {
                    sb.Append(separator);
                }

                sb.Append(s[i]);
                state = SeparatedCaseState.Lower;
            }
        }

        return sb.ToString();
    }
}
