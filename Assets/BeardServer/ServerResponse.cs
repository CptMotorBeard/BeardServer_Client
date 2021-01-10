using Newtonsoft.Json.Linq;

namespace BeardServer
{
    public enum ResponseCode
    {
        OK              = 200,
        NotFound        = 404,
        GatewayTimeout  = 504,
        Invalid         = 1000
    }

    public abstract class Response
    {
        public ResponseCode ServerResponse;
        public bool IsSuccess => ServerResponse == ResponseCode.OK;

        public Response() { }

        public void ParseResponseData(JToken json, ResponseCode responseCode)
        {
            ServerResponse = responseCode;
            if (IsSuccess)
            {
                Parse(json);
            }
        }

        protected abstract void Parse(JToken json);
    }
}