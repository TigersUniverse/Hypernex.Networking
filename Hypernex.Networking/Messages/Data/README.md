## Message Data Files

These files are to be used only to serialize data in MessagePack. 
THEY SHOULD NEVER BE SENT OVER THE SERVER ALONE! The server won't 
know what to do with them..

These files should be used in other Messages that CAN be sent over the 
server alone. For example: using a NetworkedObject in a PlayerUpdate.