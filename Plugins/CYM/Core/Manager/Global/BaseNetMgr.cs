//------------------------------------------------------------------------------
// BaseNetMgr.cs
// Copyright 2019 2019/5/8 
// Created by CYM on 2019/5/8
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

namespace CYM
{
    //public class MsgBase : MessageBase
    //{
    //    public uint netId;
    //    public NetworkHash128 assetId;
    //    public Vector3 position;
    //    public byte[] payload;

    //    // This method would be generated
    //    public override void Deserialize(NetworkReader reader)
    //    {
    //        netId = reader.ReadPackedUInt32();
    //        assetId = reader.ReadNetworkHash128();
    //        position = reader.ReadVector3();
    //        payload = reader.ReadBytesAndSize();
    //    }

    //    // This method would be generated
    //    public override void Serialize(NetworkWriter writer)
    //    {
    //        writer.WritePackedUInt32(netId);
    //        writer.Write(assetId);
    //        writer.Write(position);
    //        writer.WriteBytesFull(payload);
    //    }
    //}

    public class BaseNetMgr : BaseGFlowMgr
    {
        //#region Rec
        //public static readonly int MaxBufferSize = 1024;
        //int recHostId;
        //int recConnnectionId;
        //int recChannelId;
        //int recDataSize;
        //byte[] recBuffer = new byte[MaxBufferSize];
        //byte recError;
        //#endregion

        //#region Host
        //public GlobalConfig GlobalConfig { get; private set; }
        //public ConnectionConfig ConnectionConfig { get; private set; }
        //public HostTopology Topology { get; private set; }
        //public int ReiliableChannelId { get; private set; }
        //public int UnreliableChannelId { get; private set; }
        //public int MaxConnnectionCount { get; private set; }
        //public int Port { get; private set; }
        //public int HostId { get; private set; }
        //public int ConnectionId { get; private set; }
        //#endregion

        //#region set
        //public void CreateHost()
        //{
        //    ReiliableChannelId = ConnectionConfig.AddChannel(QosType.Reliable);
        //    UnreliableChannelId = ConnectionConfig.AddChannel(QosType.Unreliable);
        //    Topology = new HostTopology(ConnectionConfig, MaxConnnectionCount);
        //    HostId = NetworkTransport.AddHost(Topology, Port);
        //}
        //public void DestroyHost()
        //{
        //    NetworkTransport.RemoveHost(HostId);
        //}
        //public void Connect()
        //{
        //    byte error;
        //    ConnectionId = NetworkTransport.Connect(HostId, "localhost", Port, 0, out error);
        //}
        //public void Disconnect()
        //{
        //    byte error;
        //    NetworkTransport.Disconnect(HostId, ConnectionId, out error);
        //}
        //public void Send()
        //{
        //    byte error;
        //    NetworkTransport.Send(HostId, ConnectionId, ReiliableChannelId, null, 10, out error);
        //}
        //#endregion

        //#region life
        //protected override void OnSetNeedFlag()
        //{
        //    base.OnSetNeedFlag();
        //    NeedUpdate = true;
        //}
        //public override void OnCreate()
        //{
        //    base.OnCreate();
        //    MaxConnnectionCount = 100;
        //    Port = 8888;
        //    GlobalConfig = new GlobalConfig();
        //    ConnectionConfig = new ConnectionConfig();
        //    NetworkTransport.Init(GlobalConfig);
        //}
        //public override void OnUpdate()
        //{
        //    base.OnUpdate();
        //    NetworkEventType eventType = NetworkTransport.Receive(out recHostId, out recConnnectionId, out recChannelId, recBuffer, MaxBufferSize, out recDataSize, out recError);
        //    switch (eventType)
        //    {
        //        case NetworkEventType.BroadcastEvent:
        //            break;
        //        case NetworkEventType.ConnectEvent:
        //            OnConnectEvent();
        //            break;
        //        case NetworkEventType.DataEvent:
        //            Stream stream = new MemoryStream(recBuffer);
        //            BinaryFormatter formatter = new BinaryFormatter();
        //            MsgBase message = formatter.Deserialize(stream) as MsgBase;
        //            OnDataEvent(message);
        //            break;
        //        case NetworkEventType.DisconnectEvent:
        //            OnDisconnectEvent();
        //            break;
        //        case NetworkEventType.Nothing:
        //            break;
        //    }
        //}
        //#endregion

        //#region Callback
        //protected virtual void OnDataEvent(MsgBase msg)
        //{

        //}
        //protected virtual void OnConnectEvent()
        //{

        //}
        //protected virtual void OnDisconnectEvent()
        //{

        //}
        //#endregion
    }
}