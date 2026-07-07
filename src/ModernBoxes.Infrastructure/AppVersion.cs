using System;
using System.Reflection;

namespace ModernBoxes.Infrastructure
{
    public static class AppVersion
    {
        public static string Display =>
            "v" + (Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0");
    }
}
