using MagicLedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace MagicLedLibrary
{
    public class LedLibrary
    {

        public SendModels Send(int DISCOVERY_PORT)
        {
            SendModels sendModel = new SendModels();

            sendModel.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);

            try
            {
                sendModel.Socket.Bind(new IPEndPoint(IPAddress.Any, DISCOVERY_PORT));

                sendModel.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

                string msg = "HF-A11ASSISTHREAD";
                byte[] byteMsg = Encoding.UTF8.GetBytes(msg);

                var sendInfo = sendModel.Socket.SendTo(byteMsg, new IPEndPoint(IPAddress.Broadcast, DISCOVERY_PORT));

                sendModel.Status = "Sent socket message";
            }
            catch (Exception e)
            {
                sendModel.Status = ("Winsock error: " + e.ToString());
            }


            return sendModel;

        }

        public async Task<List<string>> Receive(IProgress<string> status, Socket socket, int DISCOVERY_PORT, int Timeout)
        {
            var foundBulbsList = new List<string>();
            while (true)
            {
                socket.ReceiveTimeout = Timeout;

                try
                {
                    byte[] receiveBuffer = new byte[64];

                    var receiveEndPoint = new IPEndPoint(IPAddress.Broadcast, DISCOVERY_PORT) as EndPoint;

                    var receiveInfo = socket.ReceiveFrom(receiveBuffer, ref receiveEndPoint);

                    var receivedData = Encoding.UTF8.GetString(receiveBuffer);

                    status.Report(receivedData);

                    await Task.Delay(1);

                    foundBulbsList.Add(receivedData);
                }
                catch (Exception e)
                {
                    socket.Dispose();
                    break;
                }

            }

            return foundBulbsList;
        }

        public ConnectModels Connect(string Ip, int Port)
        {
            ConnectModels connectModels = new ConnectModels();
            try
            {
                connectModels.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                connectModels.Socket.Connect(IPAddress.Parse(Ip), Port);
                connectModels.Status = "Connected";
            }
            catch (Exception e)
            {
                connectModels.Status = e.Message;
            }


            return connectModels;
        }

        private string DeterminState(byte wwLevel, byte patternCode)
        {
            string mode = "unknown";

            if (patternCode == 0x61 || patternCode == 0x62)
            {
                if (wwLevel != 0) mode = "ww";
                else mode = "color";
            }
            else if (patternCode == 0x60) mode = "custom";
            else if (IsValidPattern(patternCode))
                mode = "present";

            return mode;
        }

        public RefreshModels Refresh(Socket socket, int MAX_BUFFER_SIZE)
        {
            RefreshModels refresh = new RefreshModels();

            var msg = new List<byte> { 0x81, 0x8a, 0x8b };
            Write(socket, msg);
            var rx = Read(socket, 14, MAX_BUFFER_SIZE);

            var power_state = rx[2];

            if (power_state == 0x23) refresh.PowerState = true;
            else refresh.PowerState = false;

            var pattern = rx[3];
            var ww_level = rx[9];
            refresh.Mode = DeterminState(ww_level, pattern);

            if (refresh.Mode == "color")
            {
                var red = rx[6];
                var green = rx[7];
                var blue = rx[8];
                refresh.CurrentColor = Color.FromArgb(Convert.ToByte(255), red, green, blue);
            }

            return refresh;
        }

        public void Write(Socket socket, List<byte> msg)
        {
            msg.Add(CheckSum(msg.ToArray()));
            socket.Send(msg.ToArray());
        }

        private List<byte> Read(Socket socket, int expected, int MAX_BUFFER_SIZE)
        {
            int remaining = expected;
            var rx = new List<byte>();
            while (remaining > 0)
            {
                var chunk = new byte[MAX_BUFFER_SIZE];
                socket.Receive(chunk);
                remaining -= chunk.Length;
                rx.AddRange(chunk);
            }
            return rx;
        }

        public void SetRGB(byte r, byte g, byte b, Socket socket, bool persist = true)
        {
            List<byte> msgList;
            if (persist) msgList = new List<byte> { 0x31 };
            else msgList = new List<byte> { 0x41 };
            msgList.AddRange(new List<byte> { r, g, b });
            msgList.AddRange(new List<byte> { 0x00, 0xf0, 0x0f });
            Write(socket, msgList);
        }


        public static bool IsValidPattern(byte pattern)
        {
            if (pattern < 0x25 || pattern > 0x38)
                return false;
            else return true;
        }

        private static byte CheckSum(byte[] array)
        {
            return array.Aggregate<byte, byte>(0, (current, b) => (byte)((current + b) & 0xff));
        }
    }
}
