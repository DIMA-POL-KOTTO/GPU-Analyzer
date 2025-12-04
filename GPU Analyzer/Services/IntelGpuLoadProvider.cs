using System;
using System.Diagnostics;
using System.Linq;

public static class IntelGpuLoadWin10
{
    private static PerformanceCounter[] counters;
    
    static IntelGpuLoadWin10()
    {
        try
        {
            var category = new PerformanceCounterCategory("GPU Engine");

            // Все instance names
            var instances = category.GetInstanceNames();

            // Фильтруем только движки Intel + 3D/render
            counters = instances
                .Where(i =>
                       i.Contains("engtype_3D") ||
                       i.Contains("engtype_Compute") ||
                       i.Contains("engtype_Render"))
                .Select(i => new PerformanceCounter("GPU Engine", "Utilization Percentage", i))
                .ToArray();
        }
        catch
        {
            counters = Array.Empty<PerformanceCounter>();
        }
    }

    public static int GetLoad()
    {
        if (counters == null || counters.Length == 0)
            return 0;

        float sum = 0;

        foreach (var c in counters)
        {
            try
            {
                sum += c.NextValue();
            }
            catch { }
        }

        // Ограничиваем 0–100
        if (sum < 0) sum = 0;
        if (sum > 100) sum = 100;

        return (int)sum;
    }
}
