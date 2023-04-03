namespace Hypernex.Networking.Libs;

public class NexboxScript
{
    public NexboxLanguage Language { get; }
    public string Script { get; }

    public NexboxScript(NexboxLanguage language, string script)
    {
        Language = language;
        Script = script;
    }
}