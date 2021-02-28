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

namespace JsonBourne.DocumentReader
{
    // this is so parsers can be reused - no point in destroying and creating them all over again

    internal sealed class ValueReaderCollection : IDisposable
    {
        public JsonNullReader NullReader { get; } = new JsonNullReader();
        public JsonBooleanReader BooleanReader { get; } = new JsonBooleanReader();
        public JsonNumberReader NumberReader { get; } = new JsonNumberReader();
        public JsonStringReader StringReader { get; } = new JsonStringReader();

        public ValueReaderCollection()
        { }

        public void Dispose()
        {
            this.NullReader.Dispose();
            this.BooleanReader.Dispose();
            this.NumberReader.Dispose();
            this.StringReader.Dispose();
        }
    }
}
