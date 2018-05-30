using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web.Http;

namespace GoSqlApi
{
    public class ApiHelper
    {
        #region Properties and Constructor

        private Socket Socket { get; set; }
        private IPAddress LocalHost { get; set; }
        private StringBuilder Command { get; set; }
        private bool HasConnectionError { get; set; }
        private string ErrorReason { get; set; }

        public ApiHelper()
        {
            try
            {
                CreateSocket();
                GetLocalHost();
                MakeSocketConnection(3333); //Need to find a way abstract this
                HasConnectionError = false;
            }
            catch
            {
                HasConnectionError = true;
            }
            
        }
        #endregion

        #region API Facing Methods

        public string ExecuteCommand(string action, string id, string value = null)
        {
            if (HasConnectionError) return "ERROR! " + ErrorReason;

            Command.AppendJoin(" ", action, id);
            if (value != null) Command.Append(value);

            return TransmitCommand();
        }

        #endregion

        #region Private Methods

        private void CreateSocket()
        {
            Socket s = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            //Time the socket out if it doesn't recieve a after 3 seconds
            s.ReceiveTimeout = 3000;

            Socket = s;
        }

        private void GetLocalHost()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            LocalHost = localIPs[0]; 
        }

        private void MakeSocketConnection(int port)
        {
            IAsyncResult result = Socket.BeginConnect(LocalHost, port, null, null);

            bool success = result.AsyncWaitHandle.WaitOne(5000, true); //Measured in milliseconds

            if (!Socket.Connected)
            {              
                Socket.Close();
                ErrorReason = "Failed to connect to server.";
                HasConnectionError = true;
                throw new ApplicationException();
            }

        }

        private byte[] EncodeMessage(string message)
        {
            return Encoding.UTF8.GetBytes(message);
        }

        private string DecodeMessage(byte[] message)
        {
            return Encoding.UTF8.GetString(message);
        }

        private string TransmitCommand()
        {
            byte[] rawData = new byte[256];
            string stringData = "";

            try
            {
                Socket.Send(EncodeMessage(Command.ToString()));
                Socket.Receive(rawData);
            }
            catch
            {
                //Send 500 response with message that connection could not be made
                Socket.Close();
                ErrorReason = "Failed to receive response from database.";
                HasConnectionError = true;
                throw new ApplicationException();
            }

            try
            {
                stringData = DecodeMessage(rawData);
            }
            catch
            {
                //Send 500 response that data is corrupted within data base
                Socket.Close();
                ErrorReason = "Data my be corrupted within database.";
                HasConnectionError = true;
                throw new ApplicationException();
            }

            Socket.Close();
            return stringData;
        }

        #endregion
    }
}
