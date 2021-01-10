using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

namespace BeardServer
{
    public class Client : MonoBehaviour, IDisposable
    {
        public static Client Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                DontDestroyOnLoad(this);
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        public const int kDataBufferSize = 4096;
        public TCP tcp;

        private bool bIsConnected = false;
        private bool bIsReconnecting = false;
        private bool bIsRunning = false;

        private delegate void PacketHandler(Packet p);
        private static Dictionary<int, PacketHandler> mPacketHandlers;

        private IPAddress   mReconnectionIP;
        private int         mReconnectionPort;

        public void ConnectToServer(IPAddress ip, int port)
        {
            if (!bIsReconnecting)
            {
                mReconnectionIP     = ip;
                mReconnectionPort   = port;
            }

            tcp.OnConnect += OnConnected;
            tcp.Connect(ip, port);
        }

        public void Dispose()
        {
            tcp.Dispose();
        }

        private void InitializePacketHandlers()
        {
            mPacketHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ServerAction.ResponseCode, ClientHandler.ReceiveResponseCode }
            };
        }

        private void OnConnected()
        {            
            bIsConnected = true;

            tcp.OnConnect       -= OnConnected;
            tcp.OnDisconnect    += Disconnect;

            Debug.Log($"[BeardServer] :: Connected to {mReconnectionIP} on port {mReconnectionPort}");
        }

        private void Disconnect()
        {
            tcp.OnDisconnect -= Disconnect;
            if (bIsConnected)
            {
                bIsConnected = false;

                if (!bIsReconnecting)
                {
                    Debug.LogError($"[BeardServer] :: Disconnected from the server");
                    Task.Run(TryReconnect);
                }
            }            
        }

        private async void TryReconnect()
        {
            bIsReconnecting = true;

            while (bIsRunning && !bIsConnected)
            {
                await Task.Delay(2000);
                Debug.LogWarning($"[BeardServer] :: Attempting reconnect");
                ConnectToServer(mReconnectionIP, mReconnectionPort);
            }

            bIsReconnecting = false;
        }

        private void Start()
        {
            tcp = new TCP();
            bIsRunning = true;

            InitializePacketHandlers();
        }

        private void Update()
        {
            ThreadHelper.UpdateMain();
        }

        private void OnApplicationQuit()
        {
            tcp.OnDisconnect -= Disconnect;

            bIsRunning = false;
            tcp.Disconnect();
            Dispose();
        }

        public class TCP : IDisposable
        {
            public TcpClient Socket => mSocket;

            public Action OnConnect;
            public Action OnDisconnect;            

            private byte[]          mReceiveBuffer;
            private NetworkStream   mStream;
            private Packet          mReceivedData;            
            private TcpClient       mSocket;

            public void Connect(IPAddress ip, int port)
            {
                mReceiveBuffer = new byte[kDataBufferSize];

                mSocket = new TcpClient
                {
                    ReceiveBufferSize   = kDataBufferSize,
                    SendBufferSize      = kDataBufferSize
                };

                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mSocket.Client = client;

                Socket.BeginConnect(ip, port, ConnectCallback, Socket);
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (Socket != null)
                    {
                        mStream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    Disconnect();
                }
            }

            public void Disconnect()
            {
                OnDisconnect?.Invoke();

                mStream.Close();
                mSocket.Close();

                mStream.Dispose();
                mSocket.Dispose();
                mReceivedData.Dispose();

                mStream = null;
                mSocket = null;
                mReceivedData = null;
                mReceiveBuffer = null;
            }

            public void Dispose()
            {
                mSocket?.Close();
                mStream?.Close();

                mSocket?.Dispose();
                mStream?.Dispose();
                mReceivedData?.Dispose();
            }

            private void ConnectCallback(IAsyncResult result)
            {
                try
                {
                    Socket.EndConnect(result);

                    if (!Socket.Connected)
                        return;

                    mStream = Socket.GetStream();
                    mReceivedData = new Packet();

                    mStream.BeginRead(mReceiveBuffer, 0, kDataBufferSize, ReceiveCallback, null);

                    OnConnect?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    Disconnect();
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = mStream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(mReceiveBuffer, data, byteLength);

                    mReceivedData.Reset(HandleData(data));
                    mStream.BeginRead(mReceiveBuffer, 0, kDataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    Disconnect();
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;

                if (mReceivedData.UnreadLength() >= 4)
                {
                    packetLength = mReceivedData.ReadInt();
                    if (packetLength <= 0)
                        return true;
                }

                while (packetLength > 0 && packetLength <= mReceivedData.UnreadLength())
                {
                    byte[] packetBytes = mReceivedData.ReadBytes(packetLength);

                    ThreadHelper.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(packetBytes))
                        {
                            int packetId = _packet.ReadInt();

                            if (mPacketHandlers.TryGetValue(packetId, out PacketHandler packetHandler))
                            {
                                packetHandler(_packet);
                            }
                            else
                            {
                                Debug.LogWarning($"[BeardServer] :: Received invalid packet id {packetId}");
                            }
                        }
                    });

                    packetLength = 0;
                    if (mReceivedData.UnreadLength() >= 4)
                    {
                        packetLength = mReceivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLength <= 1)
                    return true;

                return false;
            }
        }
    }
}