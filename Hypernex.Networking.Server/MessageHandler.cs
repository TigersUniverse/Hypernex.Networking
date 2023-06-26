using Hypernex.Networking.Messages;
using Hypernex.Networking.Messages.Data;
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
            case "Hypernex.Networking.Messages.PlayerObjectUpdate":
            {
                PlayerObjectUpdate playerObjectUpdate = (PlayerObjectUpdate) Convert.ChangeType(msgMeta.Data, typeof(PlayerObjectUpdate));
                PlayerHandler.HandlePlayerObjectUpdate(instance, playerObjectUpdate, from);
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
            case "Hypernex.Networking.Messages.WorldObjectUpdate":
            {
                WorldObjectUpdate worldObjectUpdate =
                    (WorldObjectUpdate) Convert.ChangeType(msgMeta.Data, typeof(WorldObjectUpdate));
                worldObjectUpdate!.Auth.TempToken = String.Empty;
                ObjectHandler.HandleObjectUpdateMessage(instance, worldObjectUpdate, from, channel);
                break;
            }
        }
    }

    internal static class PlayerHandler
    {
        internal static Dictionary<string, PlayerUpdate> PlayerUpdates => new (_playerUpdates);
        private static Dictionary<string, PlayerUpdate> _playerUpdates = new ();

        internal static Dictionary<string, Dictionary<string, List<NetworkedObject>>> NetworkObjects => new(_networkObjects);
        private static Dictionary<string, Dictionary<string, List<NetworkedObject>>> _networkObjects = new();

        public static void HandlePlayerUpdate(HypernexInstance instance, PlayerUpdate playerUpdate, ClientIdentifier from)
        {
            playerUpdate.Auth.TempToken = String.Empty;
            instance.BroadcastMessageWithExclusion(from, Msg.Serialize(playerUpdate), MessageChannel.Unreliable);
            if (_playerUpdates.ContainsKey(playerUpdate.Auth.UserId))
            {
                _playerUpdates[playerUpdate.Auth.UserId] = playerUpdate;
                return;
            }
            _playerUpdates.Add(playerUpdate.Auth.UserId, playerUpdate);
        }

        private static int GetNetworkObjectIndex(string instanceId, string userid, NetworkedObject networkedObject)
        {
            List<NetworkedObject> networkedObjects = NetworkObjects[instanceId][userid];
            for (int i = 0; i < networkedObjects.Count; i++)
            {
                NetworkedObject networkObject = networkedObjects.ElementAt(i);
                if (networkObject.ObjectLocation == networkedObject.ObjectLocation)
                    return i;
            }
            return -1;
        }

        internal static void RemoveInstanceFromPlayerObjects(HypernexInstance instance)
        {
            if (NetworkObjects.ContainsKey(instance.InstanceId))
                _networkObjects.Remove(instance.InstanceId);
        }

        public static void HandlePlayerObjectUpdate(HypernexInstance instance, PlayerObjectUpdate playerObjectUpdate,
            ClientIdentifier from)
        {
            playerObjectUpdate.Auth.TempToken = String.Empty;
            instance.BroadcastMessageWithExclusion(from, Msg.Serialize(playerObjectUpdate), MessageChannel.Unreliable);
            string instanceId = instance.InstanceId;
            if (string.IsNullOrEmpty(instanceId))
                return;
            if(!_networkObjects.ContainsKey(instanceId))
                _networkObjects.Add(instanceId, new Dictionary<string, List<NetworkedObject>>());
            Dictionary<string, List<NetworkedObject>> dic = _networkObjects[instanceId];
            if (dic.ContainsKey(playerObjectUpdate.Auth.UserId))
            {
                int i = GetNetworkObjectIndex(instanceId, playerObjectUpdate.Auth.UserId, playerObjectUpdate.Object);
                if(i == -1)
                    _networkObjects[instanceId][playerObjectUpdate.Auth.UserId].Add(playerObjectUpdate.Object);
                else
                {
                    _networkObjects[instanceId][playerObjectUpdate.Auth.UserId].RemoveAt(i);
                    _networkObjects[instanceId][playerObjectUpdate.Auth.UserId].Insert(i, playerObjectUpdate.Object);
                }
                return;
            }
            _networkObjects[instanceId].Add(playerObjectUpdate.Auth.UserId, new List<NetworkedObject>{playerObjectUpdate.Object});
        }

        public static void HandlePlayerVoice(HypernexInstance instance, PlayerVoice playerVoice, ClientIdentifier from)
        {
            playerVoice.Auth.TempToken = String.Empty;
            instance.BroadcastMessageWithExclusion(from, Msg.Serialize(playerVoice));
        }
    }

    internal static class ObjectHandler
    {
        private static Dictionary<string, Dictionary<string, List<WorldObjectUpdate>>> objects = new();
        public static Dictionary<string, Dictionary<string, List<WorldObjectUpdate>>> Objects => new(objects);

        internal static void RemoveInstanceFromWorldObjects(HypernexInstance instance)
        {
            if (Objects.ContainsKey(instance.InstanceId))
                objects.Remove(instance.InstanceId);
        }

        internal static void RemovePlayerFromWorldObjects(HypernexInstance instance, string userid)
        {
            Dictionary<string, List<WorldObjectUpdate>> dic = GetInstance(instance);
            if (!dic.ContainsKey(userid))
                return;
            objects[instance.InstanceId].Remove(userid);
        }

        private static Dictionary<string, List<WorldObjectUpdate>> GetInstance(HypernexInstance instance)
        {
            if(!Objects.ContainsKey(instance.InstanceId))
                objects.Add(instance.InstanceId, new Dictionary<string, List<WorldObjectUpdate>>());
            return objects[instance.InstanceId];
        }

        private static bool IsObjectClaimed(HypernexInstance instance, WorldObjectUpdate worldObjectUpdate)
        {
            Dictionary<string, List<WorldObjectUpdate>> dic = GetInstance(instance);
            foreach (KeyValuePair<string,List<WorldObjectUpdate>> inp in dic)
                foreach (WorldObjectUpdate objectUpdate in inp.Value)
                    if (objectUpdate.Object.ObjectLocation == worldObjectUpdate.Object.ObjectLocation)
                        return true;
            return false;
        }

        private static bool CanObjectBeStolen(HypernexInstance instance, WorldObjectUpdate worldObjectUpdate)
        {
            if (!IsObjectClaimed(instance, worldObjectUpdate))
                return true;
            Dictionary<string, List<WorldObjectUpdate>> dic = GetInstance(instance);
            foreach (List<WorldObjectUpdate> inp in dic.Values)
            foreach (WorldObjectUpdate objectUpdate in inp)
                if (objectUpdate.Object.ObjectLocation == worldObjectUpdate.Object.ObjectLocation && objectUpdate.CanBeStolen)
                    return true;
            return false;
        }

        private static bool IsObjectClaimedByUser(HypernexInstance instance, string userid,
            WorldObjectUpdate worldObjectUpdate)
        {
            Dictionary<string, List<WorldObjectUpdate>> dic = GetInstance(instance);
            if (!dic.ContainsKey(userid))
                return false;
            foreach (WorldObjectUpdate objectUpdate in dic[userid])
                if (objectUpdate.Object.ObjectLocation == worldObjectUpdate.Object.ObjectLocation)
                    return true;
            return false;
        }

        private static void ClaimObject(HypernexInstance instance, string userid, WorldObjectUpdate worldObjectUpdate)
        {
            //worldObjectUpdate.Action = WorldObjectAction.Claim;
            Dictionary<string, List<WorldObjectUpdate>> dic = GetInstance(instance);
            if (!dic.ContainsKey(userid))
                return;
            if (IsObjectClaimed(instance, worldObjectUpdate))
            {
                // Remove it first
                foreach (string dicKey in dic.Keys)
                    RemoveObject(instance, dicKey, worldObjectUpdate);
            }
            objects[instance.InstanceId][userid].Add(worldObjectUpdate);
        }

        private static WorldObjectUpdate GetObjectByPath(HypernexInstance instance, string path)
        {
            Dictionary<string, List<WorldObjectUpdate>> dic = GetInstance(instance);
            foreach (List<WorldObjectUpdate> inp in dic.Values)
            foreach (WorldObjectUpdate objectUpdate in inp)
                if (objectUpdate.Object.ObjectLocation == path)
                    return objectUpdate;
            return null;
        }

        private static void UpdateObject(HypernexInstance instance, string userid, WorldObjectUpdate worldObjectUpdate)
        {
            //worldObjectUpdate.Action = WorldObjectAction.Update;
            Dictionary<string, List<WorldObjectUpdate>> dic = GetInstance(instance);
            if (!dic.ContainsKey(userid))
                return;
            int i = 0;
            foreach (WorldObjectUpdate objectUpdate in new List<WorldObjectUpdate>(dic[userid]))
            {
                if (objectUpdate.Object.ObjectLocation == worldObjectUpdate.Object.ObjectLocation)
                {
                    objects[instance.InstanceId][userid].RemoveAt(i);
                    objects[instance.InstanceId][userid].Add(worldObjectUpdate);
                }
                i++;
            }
        }

        private static void MakeObjectClaimable(HypernexInstance instance, string userid, WorldObjectUpdate worldObjectUpdate)
        {
            //worldObjectUpdate.Action = WorldObjectAction.Unclaim;
            Dictionary<string, List<WorldObjectUpdate>> dic = GetInstance(instance);
            if (!dic.ContainsKey(userid))
                return;
            int i = 0;
            foreach (WorldObjectUpdate objectUpdate in new List<WorldObjectUpdate>(dic[userid]))
            {
                if (objectUpdate.Auth.UserId == userid &&
                    objectUpdate.Object.ObjectLocation == worldObjectUpdate.Object.ObjectLocation)
                {
                    objects[instance.InstanceId][userid][i].CanBeStolen = true;
                    objects[instance.InstanceId][userid][i].Auth.UserId = String.Empty;
                }
                i++;
            }
        }

        private static void RemoveObject(HypernexInstance instance, string userid, WorldObjectUpdate worldObjectUpdate)
        {
            Dictionary<string, List<WorldObjectUpdate>> dic = GetInstance(instance);
            if (!dic.ContainsKey(userid))
                return;
            int i = 0;
            foreach (WorldObjectUpdate objectUpdate in new List<WorldObjectUpdate>(dic[userid]))
            {
                if (objectUpdate.Auth.UserId == userid &&
                    objectUpdate.Object.ObjectLocation == worldObjectUpdate.Object.ObjectLocation)
                    objects[instance.InstanceId][userid].RemoveAt(i);
                i++;
            }
        }

        private static void BroadcastMessage(HypernexInstance instance, WorldObjectUpdate worldObjectUpdate,
            ClientIdentifier from, MessageChannel messageChannel) =>
            instance.BroadcastMessageWithExclusion(from, Msg.Serialize(worldObjectUpdate), messageChannel);

        public static void HandleObjectUpdateMessage(HypernexInstance instance, WorldObjectUpdate worldObjectUpdate,
            ClientIdentifier from, MessageChannel messageChannel)
        {
            string userid = worldObjectUpdate.Auth.UserId;
            Dictionary<string, List<WorldObjectUpdate>> dic = GetInstance(instance);
            if(!dic.ContainsKey(userid))
                dic.Add(userid, new List<WorldObjectUpdate>());
            switch (worldObjectUpdate.Action)
            {
                case WorldObjectAction.Claim:
                    bool notClaimed = !IsObjectClaimed(instance, worldObjectUpdate);
                    bool stealable = CanObjectBeStolen(instance, worldObjectUpdate);
                    if (notClaimed || stealable)
                    {
                        WorldObjectUpdate oldUpdate = GetObjectByPath(instance, worldObjectUpdate.Object.ObjectLocation);
                        if (stealable && oldUpdate != null && worldObjectUpdate.Auth.UserId != oldUpdate.Auth.UserId)
                        {
                            oldUpdate.Action = WorldObjectAction.Unclaim;
                            ClientIdentifier clientIdentifier =
                                instance.GetClientIdentifierFromUserId(oldUpdate.Auth.UserId);
                            instance.SendMessageToClient(clientIdentifier, Msg.Serialize(oldUpdate));
                        }
                        ClaimObject(instance, userid, worldObjectUpdate);
                        BroadcastMessage(instance, worldObjectUpdate, from, messageChannel);
                    }
                    break;
                case WorldObjectAction.Update:
                    if (IsObjectClaimedByUser(instance, userid, worldObjectUpdate))
                    {
                        UpdateObject(instance, userid, worldObjectUpdate);
                        BroadcastMessage(instance, worldObjectUpdate, from, messageChannel);
                    }
                    break;
                case WorldObjectAction.Unclaim:
                    if(IsObjectClaimedByUser(instance, userid, worldObjectUpdate))
                    {
                        MakeObjectClaimable(instance, userid, worldObjectUpdate);
                        BroadcastMessage(instance, worldObjectUpdate, from, messageChannel);
                    }
                    break;
            }
        }
    }
}