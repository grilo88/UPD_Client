using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Unicode;
using UPD_Client;

BroadcastClient broadcast = new();
//_ = broadcast.ConnectChannel("192.168.10.102", 11000);
_ = broadcast.ConnectChannel("http://ec2-52-23-232-77.compute-1.amazonaws.com/", 11000);

while (true)
{
    //broadcast.SendMessage3("mensagem teste", "teste");
    bool ping = await broadcast.PingAsync(1000);
}
//void SerializeResponse(IBufferWriter<byte> writer)
//{
//    WriteInt32(writer, 4);
//    WriteString(writer, "teste");
//}

//void WriteInt32(IBufferWriter<byte> writer, int value)
//{
//    writer.Write(BitConverter.GetBytes(value));
//}

//void WriteString(IBufferWriter<byte> writer, ReadOnlySpan<char> text)
//{
//    writer.Write<byte>(UTF8Encoding.UTF8.GetBytes(text.ToArray()));
//}