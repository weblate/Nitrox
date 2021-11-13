using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxTest.Networking
{
    [TestClass]
    public class PacketSerializationTimeTest
    {
        private const string PATH_INPUT = @"C:\Users\Jannify\Desktop\packetData\packetData.bin";
        private const string PATH_OUTPUT = @"C:\Users\Jannify\Desktop\packetData\packetData(old).csv";


        private class PacketNetworkInfo
        {
            public string Name;
            public int Amount;
            public int EntryAmount;
            public int SizeCombined;
            public int MinSize;
            public int MaxSize;
            public long SerialzationTimeCombined;
            public long DeserialzationTimeCombined;

            public int AverageSize => SizeCombined / EntryAmount;
            public long AverageSerializationTime => SerialzationTimeCombined / EntryAmount;
            public long AverageDeserializationTime => DeserialzationTimeCombined / EntryAmount;

            public PacketNetworkInfo(string name)
            {
                Name = name;
                Amount = 0;
                EntryAmount = 0;
                SizeCombined = 0;
                MinSize = int.MaxValue;
                MaxSize = int.MinValue;
                SerialzationTimeCombined = 0;
                DeserialzationTimeCombined = 0;
            }
        }
        [TestMethod]
        public void AnalyseNetworkData()
        {
            const int LOOPS = 10;
            NitroxServiceLocator.InitializeDependencyContainer(new NitroxClient.ClientAutoFacRegistrar(), new TestAutoFacRegistrar());
            NitroxServiceLocator.BeginNewLifetimeScope();
            Stopwatch stopwatch = new Stopwatch();
            Dictionary<Type, PacketNetworkInfo> packetsByTypes = new Dictionary<Type, PacketNetworkInfo>();


            Packet[] packets = ReadFromBinaryFile<Packet[]>(PATH_INPUT);

            for (int i = 0; i < LOOPS; i++)
            {
                foreach (Packet packet in packets)
                {
                    if (!packetsByTypes.TryGetValue(packet.GetType(), out PacketNetworkInfo networkInfo))
                    {
                        packetsByTypes.Add(packet.GetType(), networkInfo = new PacketNetworkInfo(packet.GetType().Name));
                    }

                    stopwatch.Restart();
                    byte[] serializedData = packet.Serialize();
                    stopwatch.Stop();
                    networkInfo.SerialzationTimeCombined += stopwatch.ElapsedMilliseconds;

                    stopwatch.Restart();
                    Packet _ = Packet.Deserialize(serializedData);
                    stopwatch.Stop();
                    networkInfo.DeserialzationTimeCombined += stopwatch.ElapsedMilliseconds;


                    if (i == 0)
                    {
                        networkInfo.Amount++;
                    }
                    networkInfo.EntryAmount++;
                    networkInfo.SizeCombined += serializedData.Length;
                    if (serializedData.Length < networkInfo.MinSize)
                    {
                        networkInfo.MinSize = serializedData.Length;
                    }
                    if (serializedData.Length > networkInfo.MaxSize)
                    {
                        networkInfo.MaxSize = serializedData.Length;
                    }
                }
                NitroxModel.Logger.Log.Info($"Run {i} of {LOOPS}");
            }

            StringBuilder output = new StringBuilder("Packet Name;Amount;Average Size;Combined Size;Minimum Size;Maximum Size;Average Serialzation Time;Average Deserialzation Time\n");
            foreach (PacketNetworkInfo packetData in packetsByTypes.Values)
            {
                output.AppendLine($"{packetData.Name};{packetData.Amount};{packetData.AverageSize};{packetData.SizeCombined};{packetData.MinSize};{packetData.MaxSize};{packetData.AverageSerializationTime};{packetData.AverageDeserializationTime}");
            }

            File.WriteAllText(PATH_OUTPUT, output.ToString());
        }

        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }
    }
}
