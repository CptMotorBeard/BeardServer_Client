using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BeardServer
{
    public class ClientSend
    {
        private static ulong mResponseIndex = 0;

        private static Task<TResponse> SendTransmissionToServer<TResponse>(ClientAction action, JObject transmissionData = null) where TResponse : Response, new()
        {
            TaskCompletionSource<TResponse> taskCompletion = new TaskCompletionSource<TResponse>();
            SendTCPData(taskCompletion, action, transmissionData);

            return taskCompletion.Task;
        }

        private static void SendTCPData<TResponse>(TaskCompletionSource<TResponse> taskCompletion, ClientAction action, JObject transmissionData) where TResponse : Response, new()
        {
            Action<JToken, ResponseCode> responseAction = (JToken responseData, ResponseCode responseCode) =>
            {
                TResponse res = new TResponse();
                res.ParseResponseData(responseData, responseCode);

                taskCompletion.SetResult(res);
            };

            ClientHandler.AwaitedResponses.Add(mResponseIndex, responseAction);

            ThreadHelper.ExecuteOnMainThread(() =>
            {
                using (Packet p = new Packet((int)action))
                {
                    JObject finalTransmission = new JObject();
                    finalTransmission.Add(NetworkKeys.kResponseId, mResponseIndex);

                    if (transmissionData != null)
                        finalTransmission.Add(NetworkKeys.kData, transmissionData);

                    p.Write(finalTransmission);
                    p.WriteLength();

                    Client.Instance.tcp.SendData(p);

                    ++mResponseIndex;
                }
            });
        }
    }
}