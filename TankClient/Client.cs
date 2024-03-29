﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace TankClient
{
    internal class Client
    {
        public static Client instance = null;
        public const int DataBufferSize = 4096;
        public readonly int port;
        public readonly string ip;
        public TCP tcp;
        public int id;
        public string username;

        public int roomId;
        public string roomName;
        public int hostId;

        public List<Player> players;
        public List<Room> rooms;


        public Client(string _ip, int _port)
        {
            instance = this;
            ip = _ip;
            port = _port;
            tcp = new TCP(_ip, _port);
            tcp.Connect();
            players = new List<Player>();
            rooms = new List<Room>();
        }

        public class TCP
        {
            public TcpClient socket;
            private readonly string ip;
            private readonly int port;

            private NetworkStream stream;
            private byte[] buffer;

            public TCP(string _ip, int _port)
            {
                ip = _ip;
                port = _port;
            }

            public void Connect()
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = DataBufferSize,
                    SendBufferSize = DataBufferSize
                };

                buffer = new byte[DataBufferSize];
                socket.Connect(ip, port);
                if (!socket.Connected)
                {
                    Console.WriteLine("Can't connect to server. Please try again.");
                    return;
                }
                Console.WriteLine("Connected to server.");
                stream = socket.GetStream();

                stream.BeginRead(buffer, 0, DataBufferSize, ReceiveCallback, null);
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _bufferSize = stream.EndRead(_result);
                    if (_bufferSize <= 0)
                    {
                        instance.Disconnect();
                        return;
                    }

                    byte[] _data = new byte[_bufferSize];

                    Array.Copy(buffer, _data, _bufferSize);

                    stream.BeginRead(buffer, 0, DataBufferSize, ReceiveCallback, null);

                    PacketHandler.Handle(_data);
                }
                catch (Exception _ex)
                {
                    Console.WriteLine(_ex.Message);
                    instance.Disconnect();
                }
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    byte[] _buffer = _packet.ToArray();
                    if (socket != null)
                    {
                        stream.BeginWrite(_buffer, 0, _buffer.Length, null, null);
                    }
                }
                catch (Exception _ex)
                {
                    instance.Disconnect();
                    Console.WriteLine($"Error sending data to server via TCP: {_ex.Message}");
                }
            }

            public void Disconnect()
            {
                socket.Close();
                stream = null;
                buffer = null;
                socket = null;
            }
        }

        public void Disconnect()
        {
            tcp.Disconnect();
            instance = null;
            Console.WriteLine("Disconnected.");
        }
    }

    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NumberOfMembers { get; set; }

        public Room(int _id, string _name, int _numberOfMembers)
        {
            Id = _id;
            Name = _name;
            NumberOfMembers = _numberOfMembers;
        }

        public void Print()
        {
            Console.WriteLine($"{Id}: Room {Name} has {NumberOfMembers} members.");
        }
    }
}
