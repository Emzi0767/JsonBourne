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
using System.IO;
using JsonBourne.DocumentModel;

namespace JsonBourne.Workbench
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            byte[] dataRaw;
            using (var fs = File.OpenRead("dumpraw.json"))
            {
                dataRaw = new byte[fs.Length];
                fs.Read(dataRaw.AsSpan());
            }

            var jsonParser = new JsonParser();
            var json = jsonParser.Parse(dataRaw.AsSpan());

            if (json is not JsonObjectValue jsonObject || jsonObject.Count <= 0)
                throw new Exception("Failed to load.");
        }
    }
}
