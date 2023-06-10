using Hypernex.Networking.Messages;
using Hypernex.Networking.Messages.Data;

namespace Hypernex.Networking.Server.SandboxedClasses;

public class NetPlayer
{
    public NetPlayer()
    {
        throw new Exception("Cannot instance NetPlayer!");
    }

    private PlayerUpdate playerUpdate;

    internal NetPlayer(PlayerUpdate playerUpdate) => this.playerUpdate = playerUpdate;

    public bool IsPlayerVR => playerUpdate.IsPlayerVR;
    public string AvatarId => playerUpdate.AvatarId;
    public string[] PlayerAssignedTags => new List<string>(playerUpdate.PlayerAssignedTags).ToArray();
    public OfflineNetworkedObject[] TrackedObjects
    {
        get
        {
            List<OfflineNetworkedObject> offlineNetworkedObjects = new();
            foreach (NetworkedObject networkedObject in new List<NetworkedObject>(playerUpdate.TrackedObjects))
                offlineNetworkedObjects.Add(new OfflineNetworkedObject(networkedObject));
            return offlineNetworkedObjects.ToArray();
        }
    }
    public Dictionary<string, float> WeightedObjects => new (playerUpdate.WeightedObjects);
    public Dictionary<string, object> ExtraneousData => new(playerUpdate.ExtraneousData);
}