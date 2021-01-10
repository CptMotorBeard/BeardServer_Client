namespace BeardServer
{
    public enum ServerAction
    {
        ResponseCode
    }

    public enum ClientAction
    {

    }

    public static class NetworkKeys
    {
        public const string kResponseId = "rspid";
        public const string kResponseCode = "res";
        public const string kData = "data";
    }
}