using Hypernex.CCK;

namespace Hypernex.Networking.Server;

public class ServerLogger : Logger
{
    public override void Debug(object o)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(o);
        Console.ForegroundColor = ConsoleColor.White;
    }

    public override void Log(object o)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(o);
        Console.ForegroundColor = ConsoleColor.White;
    }

    public override void Warn(object o)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(o);
        Console.ForegroundColor = ConsoleColor.White;
    }

    public override void Error(object o)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(o);
        Console.ForegroundColor = ConsoleColor.White;
    }

    public override void Critical(Exception e)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(e);
        Console.ForegroundColor = ConsoleColor.White;
    }
}