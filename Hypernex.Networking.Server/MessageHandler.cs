using Hypernex.Networking.Messages;
using Nexport;

namespace Hypernex.Networking.Server;

public static class MessageHandler
{
    public static void HandleMessage(HypernexInstance instance, MsgMeta msgMeta, MessageChannel channel,
        ClientIdentifier from)
    {
        switch (msgMeta.DataId)
        {
            case "Hypernex.Networking.Messages.PlayerUpdate":
            {
                PlayerUpdate playerUpdate = (PlayerUpdate) Convert.ChangeType(msgMeta.Data, typeof(PlayerUpdate));
                PlayerHandler.HandlePlayerUpdate(instance, playerUpdate, from);
                break;
            }
            case "Hypernex.Networking.Messages.PlayerVoice":
            {
                PlayerVoice playerVoice = (PlayerVoice) Convert.ChangeType(msgMeta.Data, typeof(PlayerVoice));
                PlayerHandler.HandlePlayerVoice(instance, playerVoice, from);
                break;
            }
            case "Hypernex.Networking.Messages.PlayerMessage":
            {
                PlayerMessage playerMessage = (PlayerMessage) Convert.ChangeType(msgMeta.Data, typeof(PlayerMessage));
                playerMessage!.Auth.TempToken = String.Empty;
                instance.BroadcastMessageWithExclusion(from, Msg.Serialize(playerMessage));
                break;
            }
        }
    }

    private static class PlayerHandler
    {
        public static void HandlePlayerUpdate(HypernexInstance instance, PlayerUpdate playerUpdate, ClientIdentifier from)
        {
            playerUpdate.Auth.TempToken = String.Empty;
            instance.BroadcastMessageWithExclusion(from, Msg.Serialize(playerUpdate), MessageChannel.Unreliable);
        }

        public static void HandlePlayerVoice(HypernexInstance instance, PlayerVoice playerVoice, ClientIdentifier from)
        {
            playerVoice.Auth.TempToken = String.Empty;
            instance.BroadcastMessageWithExclusion(from, Msg.Serialize(playerVoice));
        }
    }
}