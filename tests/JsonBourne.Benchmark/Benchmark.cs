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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using JsonBourne.DocumentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonBourne
{
    public sealed class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            this.AddJob(Job.MediumRun
                .WithRuntime(CoreRuntime.Core50)
                .WithJit(Jit.RyuJit)
                .WithPlatform(Platform.X64));
        }
    }

    [Config(typeof(BenchmarkConfig)), MemoryDiagnoser, CategoriesColumn, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public abstract class DeserializeTreeBenchmark
    {
        public byte[] DataRaw { get; set; }
        public byte[] DataFormatted { get; set; }

        public string StringRaw { get; set; }
        public string StringFormatted { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var utf8 = new UTF8Encoding(false);

            using (var fs = File.OpenRead("dumpraw.json"))
            {
                this.DataRaw = new byte[fs.Length];
                fs.Read(this.DataRaw.AsSpan());
                this.StringRaw = utf8.GetString(this.DataRaw);
            }

            using (var fs = File.OpenRead("dump.json"))
            {
                this.DataFormatted = new byte[fs.Length];
                fs.Read(this.DataFormatted.AsSpan());
                this.StringFormatted = utf8.GetString(this.DataFormatted);
            }
        }

        [Benchmark(Baseline = true, Description = "Json.NET.raw.file"), BenchmarkCategory("Deserialize file raw stream")]
        public async Task DeserializeRawFileJsonNetAsync()
        {
            using var fs = File.OpenRead("dumpraw.json");
            using var sr = new StreamReader(fs, new UTF8Encoding(false));
            using var jsonReader = new JsonTextReader(sr);

            var jo = await JObject.LoadAsync(jsonReader);
            if (jo.Count <= 0)
                throw new Exception("Failed to load.");
        }

        [Benchmark(Baseline = false, Description = "JsonBourne.raw.file"), BenchmarkCategory("Deserialize file raw stream")]
        public async Task DeserializeRawFileJsonBourneAsync()
        {
            using var fs = File.OpenRead("dumpraw.json");
            var jsonParser = new JsonParser();
            var json = await jsonParser.ParseAsync(fs);

            if (json is not JsonObjectValue jsonObject || jsonObject.Count <= 0)
                throw new Exception("Failed to load.");
        }

        [Benchmark(Baseline = false, Description = "STJ.raw.file"), BenchmarkCategory("Deserialize file raw stream")]
        public async Task DeserializeRawFileSTJAsync()
        {
            using var fs = File.OpenRead("dumpraw.json");
            var json = await JsonDocument.ParseAsync(fs);

            if (json.RootElement.ValueKind != JsonValueKind.Object)
                throw new Exception("Failed to load.");
        }

        [Benchmark(Baseline = true, Description = "Json.NET.raw.buff"), BenchmarkCategory("Deserialize file raw buffer")]
        public void DeserializeRawBufferJsonNet()
        {
            var jo = JObject.Parse(this.StringRaw);
            if (jo.Count <= 0)
                throw new Exception("Failed to load.");
        }

        [Benchmark(Baseline = false, Description = "JsonBourne.raw.buff"), BenchmarkCategory("Deserialize file raw buffer")]
        public void DeserializeRawBufferJsonBourne()
        {
            var jsonParser = new JsonParser();
            var json = jsonParser.Parse(this.DataRaw.AsSpan());

            if (json is not JsonObjectValue jsonObject || jsonObject.Count <= 0)
                throw new Exception("Failed to load.");
        }

        [Benchmark(Baseline = false, Description = "STJ.raw.buff"), BenchmarkCategory("Deserialize file raw buffer")]
        public void DeserializeRawBufferSTJ()
        {
            var json = JsonDocument.Parse(this.DataRaw.AsMemory());

            if (json.RootElement.ValueKind != JsonValueKind.Object)
                throw new Exception("Failed to load.");
        }

        [Benchmark(Baseline = true, Description = "Json.NET.formatted.file"), BenchmarkCategory("Deserialize file formatted stream")]
        public async Task DeserializeFormattedFileJsonNetAsync()
        {
            using var fs = File.OpenRead("dump.json");
            using var sr = new StreamReader(fs, new UTF8Encoding(false));
            using var jsonReader = new JsonTextReader(sr);

            var jo = await JObject.LoadAsync(jsonReader);
            if (jo.Count <= 0)
                throw new Exception("Failed to load.");
        }

        [Benchmark(Baseline = false, Description = "JsonBourne.formatted.file"), BenchmarkCategory("Deserialize file formatted stream")]
        public async Task DeserializeFormattedFileJsonBourneAsync()
        {
            using var fs = File.OpenRead("dump.json");
            var jsonParser = new JsonParser();
            var json = await jsonParser.ParseAsync(fs);

            if (json is not JsonObjectValue jsonObject || jsonObject.Count <= 0)
                throw new Exception("Failed to load.");
        }

        [Benchmark(Baseline = false, Description = "STJ.formatted.file"), BenchmarkCategory("Deserialize file formatted stream")]
        public async Task DeserializeFormattedFileSTJAsync()
        {
            using var fs = File.OpenRead("dump.json");
            var json = await JsonDocument.ParseAsync(fs);

            if (json.RootElement.ValueKind != JsonValueKind.Object)
                throw new Exception("Failed to load.");
        }

        [Benchmark(Baseline = true, Description = "Json.NET.formatted.buff"), BenchmarkCategory("Deserialize file formatted buffer")]
        public void DeserializeFormattedBufferJsonNet()
        {
            var jo = JObject.Parse(this.StringFormatted);
            if (jo.Count <= 0)
                throw new Exception("Failed to load.");
        }

        [Benchmark(Baseline = false, Description = "JsonBourne.formatted.buff"), BenchmarkCategory("Deserialize file formatted buffer")]
        public void DeserializeFormattedBufferJsonBourne()
        {
            var jsonParser = new JsonParser();
            var json = jsonParser.Parse(this.DataFormatted.AsSpan());

            if (json is not JsonObjectValue jsonObject || jsonObject.Count <= 0)
                throw new Exception("Failed to load.");
        }

        [Benchmark(Baseline = false, Description = "STJ.formatted.buff"), BenchmarkCategory("Deserialize file formatted buffer")]
        public void DeserializeFormattedBufferSTJ()
        {
            var json = JsonDocument.Parse(this.DataFormatted.AsMemory());

            if (json.RootElement.ValueKind != JsonValueKind.Object)
                throw new Exception("Failed to load.");
        }

        [Benchmark(Baseline = true, Description = "Json.NET.both.file"), BenchmarkCategory("Deserialize file both stream")]
        public async Task DeserializeBothFileJsonNetAsync()
        {
            using (var fs = File.OpenRead("dumpraw.json"))
            {
                using var sr = new StreamReader(fs, new UTF8Encoding(false));
                using var jsonReader = new JsonTextReader(sr);

                var jo = await JObject.LoadAsync(jsonReader);
                if (jo.Count <= 0)
                    throw new Exception("Failed to load.");
            }

            using (var fs = File.OpenRead("dump.json"))
            {
                using var sr = new StreamReader(fs, new UTF8Encoding(false));
                using var jsonReader = new JsonTextReader(sr);

                var jo = await JObject.LoadAsync(jsonReader);
                if (jo.Count <= 0)
                    throw new Exception("Failed to load.");
            }
        }

        [Benchmark(Baseline = false, Description = "JsonBourne.both.file"), BenchmarkCategory("Deserialize file both stream")]
        public async Task DeserializeBothFileJsonBourneAsync()
        {
            var jsonParser = new JsonParser();

            using (var fs = File.OpenRead("dumpraw.json"))
            {
                var json = await jsonParser.ParseAsync(fs);

                if (json is not JsonObjectValue jsonObject || jsonObject.Count <= 0)
                    throw new Exception("Failed to load.");
            }

            using (var fs = File.OpenRead("dump.json"))
            {
                var json = await jsonParser.ParseAsync(fs);

                if (json is not JsonObjectValue jsonObject || jsonObject.Count <= 0)
                    throw new Exception("Failed to load.");
            }
        }

        [Benchmark(Baseline = false, Description = "STJ.both.file"), BenchmarkCategory("Deserialize file both stream")]
        public async Task DeserializeBothFileSTJAsync()
        {
            using (var fs = File.OpenRead("dumpraw.json"))
            {
                var json = await JsonDocument.ParseAsync(fs);

                if (json.RootElement.ValueKind != JsonValueKind.Object)
                    throw new Exception("Failed to load.");
            }

            using (var fs = File.OpenRead("dump.json"))
            {
                var json = await JsonDocument.ParseAsync(fs);

                if (json.RootElement.ValueKind != JsonValueKind.Object)
                    throw new Exception("Failed to load.");
            }
        }

        [Benchmark(Baseline = true, Description = "Json.NET.both.buff"), BenchmarkCategory("Deserialize file both buffer")]
        public void DeserializeBothBufferJsonNet()
        {
            var jo = JObject.Parse(this.StringRaw);
            if (jo.Count <= 0)
                throw new Exception("Failed to load.");

            jo = JObject.Parse(this.StringFormatted);
            if (jo.Count <= 0)
                throw new Exception("Failed to load.");
        }

        [Benchmark(Baseline = false, Description = "JsonBourne.both.buff"), BenchmarkCategory("Deserialize file both buffer")]
        public void DeserializeBothBufferJsonBourne()
        {
            var jsonParser = new JsonParser();

            var json = jsonParser.Parse(this.DataRaw.AsSpan());
            if (json is not JsonObjectValue jsonObject || jsonObject.Count <= 0)
                throw new Exception("Failed to load.");

            json = jsonParser.Parse(this.DataFormatted.AsSpan());
            if (json is not JsonObjectValue jsonObject2 || jsonObject2.Count <= 0)
                throw new Exception("Failed to load.");
        }

        [Benchmark(Baseline = false, Description = "STJ.both.buff"), BenchmarkCategory("Deserialize file both buffer")]
        public void DeserializeBothBufferSTJ()
        {
            var json = JsonDocument.Parse(this.DataRaw.AsMemory());

            if (json.RootElement.ValueKind != JsonValueKind.Object)
                throw new Exception("Failed to load.");

            json = JsonDocument.Parse(this.DataFormatted.AsMemory());

            if (json.RootElement.ValueKind != JsonValueKind.Object)
                throw new Exception("Failed to load.");
        }
    }
}
