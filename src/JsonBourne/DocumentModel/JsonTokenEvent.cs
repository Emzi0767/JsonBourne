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

namespace JsonBourne.DocumentModel
{
    internal class JsonTokenEvent
    {
        public static JsonTokenEvent None { get; } = new JsonTokenEvent(JsonTokenType.None, null, 0, 0, 0);
        public static JsonTokenEvent EOF { get; } = new JsonTokenEvent(JsonTokenType.EndOfStream, null, 0, 0, 0);

        public JsonTokenType Type { get; }

        public int StreamPosition { get; }

        public int Line { get; }

        public int Column { get; }

        public object Value { get; }

        public JsonTokenEvent(JsonTokenType type, object value, int streamPos, int col, int line)
        {
            this.Type = type;
            this.Value = value;
            this.StreamPosition = streamPos;
            this.Column = col;
            this.Line = line;
        }
    }
}
