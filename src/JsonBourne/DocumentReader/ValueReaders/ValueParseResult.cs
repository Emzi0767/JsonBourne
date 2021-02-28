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

using System.Text;

namespace JsonBourne.DocumentReader
{
    internal class ValueParseResult
    {
        public static ValueParseResult Success { get; } = new ValueParseResult { Type = ValueParseResultType.Success };
        public static ValueParseResult EOF { get; } = new ValueParseResult { Type = ValueParseResultType.EOF, IsEOF = true };
        public static ValueParseResult Indeterminate { get; } = new ValueParseResult { Type = ValueParseResultType.Intederminate };
        public static ValueParseResult FailureEOF { get; } = new ValueParseResult { Type = ValueParseResultType.Failure, Reason = "Unexpected EOF.", IsEOF = true };

        public ValueParseResultType Type { get; init; }
        public string Reason { get; init; }
        public Rune FailingRune { get; init; }
        public bool IsEOF { get; init; }

        public int StreamPosition { get; private set; } = -1;
        public int Line { get; private set; } = -1;
        public int Column { get; private set; } = -1;

        public static ValueParseResult Failure(string reason, Rune failingRune)
            => new ValueParseResult { Type = ValueParseResultType.Failure, Reason = reason, FailingRune = failingRune, StreamPosition = -1, Line = -1, Column = -1 };

        public ValueParseResult Enrich(int pos, int line, int col)
        {
            this.StreamPosition = pos;
            this.Line = line;
            this.Column = col;
            return this;
        }

        public override bool Equals(object obj)
            => obj is ValueParseResult other && other == this;

        public override int GetHashCode()
            => this.Type.GetHashCode();

        public static bool operator ==(ValueParseResult left, ValueParseResult right)
            => left.Type == right.Type;

        public static bool operator !=(ValueParseResult left, ValueParseResult right)
            => left.Type != right.Type;
    }

    internal enum ValueParseResultType
    {
        Success,
        Failure,
        EOF,
        Intederminate
    }
}
