using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace BeardServer
{
    public class ClientHandler
    {
        public static Dictionary<ulong, Action<JToken, ResponseCode>> AwaitedResponses = new Dictionary<ulong, Action<JToken, ResponseCode>>();

        public static void ReceiveResponseCode(Packet p)
        {
            JObject json = p.Read();

            ulong id            = json[NetworkKeys.kResponseId].ToObject<ulong>();
            int responseCode    = json[NetworkKeys.kResponseCode].ToObject<int>();

            JToken body = "";
            if (json.ContainsKey(NetworkKeys.kData))
            {
                body = json[NetworkKeys.kData];
            }

            if (AwaitedResponses.ContainsKey(id))
            {
                ResponseCode r = (ResponseCode)responseCode;
                AwaitedResponses[id](body, r);

                AwaitedResponses.Remove(id);
            }
        }
    }
}