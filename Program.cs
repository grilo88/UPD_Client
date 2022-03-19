using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Unicode;
using UPD_Client;

BroadcastClient broadcast = new();
_ = broadcast.ConnectChannel("192.168.10.102", 11000, false);
//_ = broadcast.ConnectChannel("http://ec2-52-23-232-77.compute-1.amazonaws.com/", 11000, false);

while (true)
{
    await broadcast.SendLargeData("teste");
    //broadcast.SendMessage3("mensagem teste", "teste");
    // bool ping = await broadcast.PingAsync(1000);
}