using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using shared;

namespace BeardServer
{
    public class ClientSend
    {
        public static Task<GeneralResponse> SendGeneralRequestToServer()
        {
            return SendTransmissionToServer<GeneralResponse>(NetworkActions.kGeneralResponse);
        }

        private static int mResponseIndex = 0;

        private static Task<TResponse> SendTransmissionToServer<TResponse>(string action, JObject transmissionData = null) where TResponse : Response, new()
        {
            TaskCompletionSource<TResponse> taskCompletion = new TaskCompletionSource<TResponse>();
            SendTCPData(taskCompletion, action, transmissionData);

            return taskCompletion.Task;
        }

        private static void SendTCPData<TResponse>(TaskCompletionSource<TResponse> taskCompletion, string action, JObject transmissionData) where TResponse : Response, new()
        {
            Action<JToken, BeardServerManager.ResponseCodes> responseAction = (JToken responseData, BeardServerManager.ResponseCodes responseCode) =>
            {
                TResponse res = new TResponse();
                res.ParseResponseData(responseData, responseCode);

                taskCompletion.SetResult(res);
            };

            ClientHandler.AwaitedResponses.Add(mResponseIndex, responseAction);

            ThreadHelper.ExecuteOnMainThread(() =>
            {
                using (Packet p = new Packet(mResponseIndex))
                {
                    JObject finalTransmission = new JObject();
                    finalTransmission.Add(NetworkKeys.kAction, action);

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