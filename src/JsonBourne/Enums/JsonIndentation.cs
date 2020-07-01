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

namespace JsonBourne
{
    /// <summary>
    /// Defines the kind of indentation to emit when writing JSON.
    /// </summary>
    public enum JsonIndentation : int
    {
        /// <summary>
        /// Specifies that no identation is to be used. This means no line breaks or whitespace at the beginning or end of each line.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that the resulting JSON is to be indented using just linebreaks. No whitespace will be emitted at the beginning of each line.
        /// </summary>
        LineBreaks = 1,

        /// <summary>
        /// Defines that the resulting JSON will use one tabulation (\t) character per indentation level. Line breaks will be emitted at the end of each line.
        /// </summary>
        Tabs = 2,

        /// <summary>
        /// Defines that the resulting JSON will use one space character (\x20) character per indentation level. Line breaks will be emitted at the end of each line.
        /// </summary>
        OneSpace = 3,

        /// <summary>
        /// Defines that the resulting JSON will use two space characters (\x20) character per indentation level. Line breaks will be emitted at the end of each line.
        /// </summary>
        TwoSpaces = 4,

        /// <summary>
        /// Defines that the resulting JSON will use four space characters (\x20) character per indentation level. Line breaks will be emitted at the end of each line.
        /// </summary>
        FourSpaces = 5,

        /// <summary>
        /// Defines that the resulting JSON will use eight space characters (\x20) character per indentation level. Line breaks will be emitted at the end of each line.
        /// </summary>
        EightSpaces = 6
    }
}
