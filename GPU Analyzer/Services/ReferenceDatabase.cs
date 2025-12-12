using GPU_Analyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace GPU_Analyzer.Services
{
    public class ReferenceDatabase
    {
        public Dictionary<string, ReferenceValues> Data { get; set; }

        public static ReferenceDatabase Load(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ReferenceDatabase>(json);
        }

        public ReferenceValues GetValues(string gpuName)
        {
            Data.TryGetValue(gpuName, out var result);
            return result;
        }
    }
}
