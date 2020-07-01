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

namespace JsonBourne
{
    /// <summary>
    /// Defines how special values (i.e. <see cref="float.NaN"/>, <see cref="float.PositiveInfinity"/>, <see cref="float.NegativeInfinity"/>, and their <see cref="double"/> equivalents are emitted in JSON.
    /// </summary>
    public enum JsonFloatSpecialValueEncoding
    {
        /// <summary>
        /// <para>Defines that these values will be emitted as strings.</para>
        /// </summary>
        String = 0,

        /// <summary>
        /// <para>Defines that these values will be replaced with default value for given type.</para>
        /// <para>For <see cref="float"/> and <see cref="double"/>, this is going to be 0.0, and for <see cref="Nullable{T}"/> it's going to be <see langword="null"/>.</para>
        /// </summary>
        DefaultT = 1
    }
}
