// OSC Jack - Open Sound Control plugin for Unity
// https://github.com/keijiro/OscJack

using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace OscJack
{
    public sealed class OscClient : IDisposable
    {
        #region Object life cycle

        public OscClient(string destination, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            if (destination == "255.255.255.255")
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

            var dest = new IPEndPoint(IPAddress.Parse(destination), port);
            _socket.Connect(dest);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Packet sender methods

        public void Send(string address)
        {
            _encoder.Clear();
            _encoder.Append(address);
            _encoder.Append(",");
            _socket.Send(_encoder.Buffer, _encoder.Length, SocketFlags.None);
        }

        public void Send(string address, params int[] data)
        {
            _encoder.Clear();
            _encoder.Append(address);
            _encoder.Append(",iiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii".Substring(0, data.Length + 1));
            foreach(int d in data)
                _encoder.Append(d);
            _socket.Send(_encoder.Buffer, _encoder.Length, SocketFlags.None);
        }

        public void Send(string address, params float[] data)
        {
            _encoder.Clear();
            _encoder.Append(address);
            _encoder.Append(",ffffffffffffffffffffffffffffffff".Substring(0, data.Length + 1));
            foreach(float d in data)
                _encoder.Append(d);
            _socket.Send(_encoder.Buffer, _encoder.Length, SocketFlags.None);
        }

        public bool SendCustom(string address, string format, params object[] data)
        {
            _encoder.Clear();
            _encoder.Append(address);
            _encoder.Append(format);
            if (format.Length != data.Length)
            {
                Debug.LogError("OscClient SendCustom Format and data length do not match.");
                return false;
            }
            int i = 0;
            foreach (char d in format.ToCharArray())
            {
                switch (d)
                {
                    case 'i':
                        if (data[i] is int intValue)
                            _encoder.Append(intValue);
                        else
                        {
                            Debug.LogError("OscClient SendCustom Format and data type do not match. Expected int value.");
                            return false;
                        }
                        break;
                    case 'f':
                        if (data[i] is float floatValue)
                            _encoder.Append(floatValue);
                        else
                        {
                            Debug.LogError("OscClient SendCustom Format and data type do not match. Eexpected float value.");
                            return false;
                        }
                        break;
                    case 's':
                        if (data[i] is string stringValue)
                            _encoder.Append(stringValue);
                        else
                        {
                            Debug.LogError("OscClient SendCustom Format and data type do not match. Expected string value.");
                            return false;
                        }
                        break;
                    default:
                    {
                        Debug.LogError("OscClient SendCustom " + d + " is not a valid OSC type.");
                        return false;
                    }
                }

                i++;
            }
            _socket.Send(_encoder.Buffer, _encoder.Length, SocketFlags.None);
            return true;
        }

        public void Send(string address, string data)
        {
            _encoder.Clear();
            _encoder.Append(address);
            _encoder.Append(",s");
            _encoder.Append(data);
            _socket.Send(_encoder.Buffer, _encoder.Length, SocketFlags.None);
        }

        #endregion

        #region IDispose implementation

        bool _disposed;

        void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                if (_socket != null)
                {
                    _socket.Close();
                    _socket = null;
                }

                _encoder = null;
            }
        }

        ~OscClient()
        {
            Dispose(false);
        }

        #endregion

        #region Private variables

        OscPacketEncoder _encoder = new OscPacketEncoder();
        Socket _socket;

        #endregion

        public void SendSpinMessage(string address, float positionX, float positionY, float positionZ, float rotationW, float rotationX, float rotationY, float rotationZ, float batteryValue, uint ts)
        {
            _encoder.Clear();
            _encoder.Append(address);
            _encoder.Append("ffffffffi");
            _encoder.Append(positionX);
            _encoder.Append(positionY);
            _encoder.Append(positionZ);
            _encoder.Append(rotationW);
            _encoder.Append(rotationX);
            _encoder.Append(rotationY);
            _encoder.Append(rotationZ);
            _encoder.Append(batteryValue);
            _encoder.Append((int)ts);
            _socket.Send(_encoder.Buffer, _encoder.Length, SocketFlags.None);
        }
    }
}
