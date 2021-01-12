using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

using shared;

namespace BeardServer
{
    public class ClientHandler
    {
        public static Dictionary<int, Action<JToken, BeardServerManager.ResponseCodes>> AwaitedResponses = new Dictionary<int, Action<JToken, BeardServerManager.ResponseCodes>>();

        private delegate void PacketHandler(int transmissionId, BeardServerManager.ResponseCodes response, JToken data);
        
        private static Dictionary<string, PacketHandler> mPacketHandlers = new Dictionary<string, PacketHandler>()
        {
            { NetworkActions.kGeneralResponse, ReceiveResponseCode }
        };

        public static void ReceivedPacket(Packet p, int transmissionId)
        {
            JObject json = p.Read();

            string action = json[NetworkKeys.kAction].ToObject<string>();
            BeardServerManager.ResponseCodes response = json[NetworkKeys.kResponseCode].ToObject<BeardServerManager.ResponseCodes>();

            JToken data = "";
            if (json.ContainsKey(NetworkKeys.kData))
            {
                data = json[NetworkKeys.kData];
            }

            if (mPacketHandlers.ContainsKey(action))
            {
                mPacketHandlers[action](transmissionId, response, data);
            }
        }

        public static void ReceiveResponseCode(int transmissionId, BeardServerManager.ResponseCodes response, JToken data)
        {            
            if (AwaitedResponses.ContainsKey(transmissionId))
            {
                AwaitedResponses[transmissionId](data, response);

                AwaitedResponses.Remove(transmissionId);
            }
        }
    }
}