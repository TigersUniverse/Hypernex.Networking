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
    private HypernexInstance instance;

    internal NetPlayer(HypernexInstance instance, PlayerUpdate playerUpdate)
    {
        this.instance = instance;
        this.playerUpdate = playerUpdate;
    }

    public bool IsPlayerVR => playerUpdate.IsPlayerVR;
    public string AvatarId => playerUpdate.AvatarId;
    public string[] PlayerAssignedTags => new List<string>(playerUpdate.PlayerAssignedTags).ToArray();
    public OfflineNetworkedObject[] TrackedObjects
    {
        get
        {
            List<OfflineNetworkedObject> offlineNetworkedObjects = new();
            string instanceId = instance.InstanceId;
            if(string.IsNullOrEmpty(instanceId))
                return offlineNetworkedObjects.ToArray();
            Dictionary<string, Dictionary<string, List<NetworkedObject>>> o = MessageHandler.PlayerHandler
                .NetworkObjects;
            if (!o.ContainsKey(instance.InstanceId))
                return offlineNetworkedObjects.ToArray();
            Dictionary<string, List<NetworkedObject>> n = o[instanceId];
            if(!n.ContainsKey(playerUpdate.Auth.UserId))
                return offlineNetworkedObjects.ToArray();
            List<NetworkedObject> b = n[playerUpdate.Auth.UserId];
            foreach (NetworkedObject networkedObject in new List<NetworkedObject>(b))
                offlineNetworkedObjects.Add(new OfflineNetworkedObject(networkedObject));
            return offlineNetworkedObjects.ToArray();
        }
    }
    public Dictionary<string, float> WeightedObjects => new (playerUpdate.WeightedObjects);
    public Dictionary<string, object> ExtraneousData => new(playerUpdate.ExtraneousData);
}