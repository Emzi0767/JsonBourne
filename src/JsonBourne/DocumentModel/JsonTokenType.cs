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

namespace JsonBourne.DocumentModel
{
    [Flags]
    internal enum JsonTokenType : int
    {
        None = 0,

        // ' '  SPACE
        // '\n' LINE FEED
        // '\r' CARRIAGE RETURN
        // '\t' HORIZONTAL TAB
        Whitespace = 1 << 0,

        // "..." STRING
        // Start token: "
        String = 1 << 1,

        // ONE OF:
        // -?0(?:\.[0-9]+)?(?:[eE](?:+|-)?[0-9]+)?
        // -?[1-9][0-9]*(?:\.[0-9]+)?(?:[eE](?:+|-)?[0-9]+)?
        // Start token: - 0 1 2 3 4 5 6 7 8 9
        Number = 1 << 2,

        // true
        True = 1 << 3,

        // false
        False = 1 << 4,

        // null
        Null = 1 << 5,

        // [
        ArrayStart = 1 << 6,

        // ]
        ArrayEnd = 1 << 7,

        // {
        ObjectStart = 1 << 8,

        // }
        ObjectEnd = 1 << 9,

        // ,
        ItemSeparator = 1 << 10,

        // :
        KeyValueSeparator = 1 << 11,

        // any whitespace
        // "
        // - 0 1 2 3 4 5 6 7 8 9
        // true
        // false
        // null
        // [
        // {
        Value = Whitespace | String | Number | True | False | Null | ArrayStart | ObjectStart,

        // any whitespace
        // ,
        // ]
        ArrayNext = Whitespace | ItemSeparator | ArrayEnd,

        // any whitespace
        // ,
        // }
        ObjectNext = Whitespace | ItemSeparator | ObjectEnd,

        // any whitespace
        // "
        ObjectKey = Whitespace | String,

        // any whitespace
        // :
        ObjectKeyValueSeparator = Whitespace | KeyValueSeparator,
    }
}
