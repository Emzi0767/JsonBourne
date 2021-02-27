// This file is a part of JsonBourne project.
// 
// Copyright 2020 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Text;

namespace JsonBourne
{
    /// <summary>
    /// Indicates failure to parse JSON, typically due to malformed input.
    /// </summary>
    public sealed class JsonParseException : Exception
    {
        /// <summary>
        /// Gets the position in the stream at which the exception occured. This value is 0-indexed.
        /// </summary>
        public int StreamPosition { get; }

        /// <summary>
        /// Gets the line at which the exception occured. This value is 0-indexed.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Gets the column at which the exception occured. This value is 0-indexed.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Gets the rune that caused the parsing problem. The rune might consist of more than one UTF-16 codepoint.
        /// </summary>
        public Rune Rune { get; }

        /// <summary>
        /// Gets the message reported by the parser.
        /// </summary>
        public string ParserMessage { get; }

        internal JsonParseException(Exception inner)
            : base("Exception occured while parsing JSON.", inner)
        {
            this.StreamPosition = -1;
            this.Line = -1;
            this.Column = -1;
            this.Rune = default;
            this.ParserMessage = null;
        }

        internal JsonParseException(int pos, int line, int col, Rune rune, string reason)
            : this(pos, line, col, rune, reason, null)
        { }

        internal JsonParseException(int pos, int line, int col, Rune rune, string reason, Exception inner)
            : base($"Unexpected rune in JSON input: '{rune}', line: {line}, column: {col}, stream position: {pos}, reason: '{reason}'.", inner)
        {
            this.StreamPosition = pos;
            this.Line = line;
            this.Column = col;
            this.Rune = rune;
            this.ParserMessage = reason;
        }
    }
}
