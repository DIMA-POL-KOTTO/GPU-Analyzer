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
            var instances = category.GetInstanceNames();
            counters = instances.Where(i => i.Contains("engtype_3D") || i.Contains("engtype_Compute") || i.Contains("engtype_Render"))
                .Select(i => new PerformanceCounter("GPU Engine", "Utilization Percentage", i)).ToArray();
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

        if (sum < 0) sum = 0;
        if (sum > 100) sum = 100;

        return (int)sum;
    }
}
