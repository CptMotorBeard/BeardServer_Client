using Newtonsoft.Json.Linq;
using shared;

namespace BeardServer
{    
    public abstract class Response
    {
        public BeardServerManager.ResponseCodes ServerResponse;
        public bool IsSuccess => ServerResponse == BeardServerManager.ResponseCodes.OK;

        public Response() { }

        public void ParseResponseData(JToken json, BeardServerManager.ResponseCodes responseCode)
        {
            ServerResponse = responseCode;
            if (IsSuccess)
            {
                Parse(json);
            }
        }

        protected abstract void Parse(JToken json);
    }

    public class GeneralResponse : Response
    {
        protected override void Parse(JToken json)
        {
            // No response data
        }
    }
}