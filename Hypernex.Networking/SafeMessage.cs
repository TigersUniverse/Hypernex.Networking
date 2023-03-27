using System;
using Nexport;

namespace Hypernex.Networking;

public static class SafeMessage
{
    public static (bool, T) TryGetMessage<T>(byte[] message)
    {
        try
        {
            return (true, Msg.Deserialize<T>(message));
        }
        catch (Exception)
        {
            // This will not work 100% of the time!
            return (false, Activator.CreateInstance<T>());
        }
    }
}