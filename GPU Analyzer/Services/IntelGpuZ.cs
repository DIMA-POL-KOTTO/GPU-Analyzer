using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GPU_Analyzer.Services
{
    public class IntelGpuZ
    {
        private readonly string logPath;
        public float Temperature { get; private set; }
        public float CoreClock { get; private set; }
        public float MemoryClock { get; private set; }
        public bool IsAvailable => File.Exists(logPath);
        public IntelGpuZ()
        {
            //string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            logPath = "D:\\GPU-Z Sensor Log.txt";
        }
        public bool TryUpdate()
        {
            
            if (!IsAvailable)
                return false;

            try
            {
                string[] lines;

                using (var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fs, Encoding.UTF8))
                {
                    lines = reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
                }

                if (lines.Length < 2)
                    return false;

                string last = lines.Last(); 
                string[] parts = last.Split(',', StringSplitOptions.TrimEntries);

                string header = lines[0];
                string[] headerParts = header.Split(',', StringSplitOptions.TrimEntries);

                CoreClock = ExtractColumn(headerParts, parts, "GPU Clock");
                MemoryClock = ExtractColumn(headerParts, parts, "Memory Clock");
                Temperature = ExtractColumn(headerParts, parts, "GPU Temperature");
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        private float ExtractColumn(string[] header, string[] row, string key)
        {
            for (int i = 0; i < header.Length; i++)
            {
                if (header[i].Contains(key, StringComparison.OrdinalIgnoreCase))
                {
                    if (float.TryParse(row[i].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture, out float v))
                        return v;
                }
            }
            return 0f;
        }
    }
}
