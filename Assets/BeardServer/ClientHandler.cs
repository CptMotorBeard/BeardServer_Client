using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

using shared;

namespace BeardServer
{
    public class ClientHandler
    {
        public static Dictionary<int, Action<JToken, BeardServerManager.ResponseCodes>> AwaitedResponses = new Dictionary<int, Action<JToken, BeardServerManager.ResponseCodes>>();

        public static void ReceiveResponseCode(Packet p)
        {
            JObject json = p.Read();

            int id              = json[NetworkKeys.kAction].ToObject<int>();
            int responseCode    = json[NetworkKeys.kResponseCode].ToObject<int>();

            JToken body = "";
            if (json.ContainsKey(NetworkKeys.kData))
            {
                body = json[NetworkKeys.kData];
            }

            if (AwaitedResponses.ContainsKey(id))
            {
                BeardServerManager.ResponseCodes r = (BeardServerManager.ResponseCodes)responseCode;
                AwaitedResponses[id](body, r);

                AwaitedResponses.Remove(id);
            }
        }
    }
}