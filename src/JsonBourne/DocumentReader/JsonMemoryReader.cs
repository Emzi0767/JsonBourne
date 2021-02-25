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
using System.Collections;
using System.Collections.Generic;
using JsonBourne.DocumentModel;

namespace JsonBourne.DocumentReader
{
    internal sealed class JsonMemoryReader : IEnumerator<JsonTokenEvent>
    {
        public JsonTokenEvent Current { get; private set; } = JsonTokenEvent.None;

        object IEnumerator.Current
            => this.Current;

        public int Position { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public int BufferPosition { get; private set; }

        internal Memory<byte> Buffer
        {
            get => this._buff;
            set
            {
                this.BufferPosition -= this._buff.Length + 1;
                if (this.BufferPosition < -1)
                    this.BufferPosition = -1;

                this._buff = value;
            }
        }
        private Memory<byte> LastToken { get; set; }

        private Memory<byte> _buff;

        public JsonMemoryReader()
        {
            this.Reset();
        }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Advance(int amount, int lines, int cols)
        {
            this.Line += lines;
            if (lines > 0)
                this.Column = cols;
            else
                this.Column += cols;

            this.BufferPosition += amount;
            this.Position += amount;
        }

        public void Reset()
        {
            this.BufferPosition = -1;
            this.Position = -1;
            this.Column = -1;
            this.Line = 0;
            this.LastToken = default;
            this.Current = JsonTokenEvent.None;
        }

        public void Dispose()
            => this.Reset();
    }
}
