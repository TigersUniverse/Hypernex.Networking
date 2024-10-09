using System;
using System.Globalization;

namespace Hypernex.Sandboxing.SandboxedTypes;

public static class TimeSettings
{
    public static Func<bool> Is24HourClock = () => DateTimeFormatInfo.CurrentInfo.ShortTimePattern.Contains("H");
}