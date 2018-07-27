﻿using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using IonDotnet.Internals;
using IonDotnet.Internals.Binary;
using IonDotnet.Serialization;
using Newtonsoft.Json;

namespace IonDotnet.Bench
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SerializerGround : IRunable
    {
        private class SimplePoco
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Nickname { get; set; }
            public int Id { get; set; }
            public bool IsHandsome { get; set; }
        }

        [MemoryDiagnoser]
        public class Benchmark
        {
            private static readonly List<SimplePoco> Data = GenerateArray();
            private const int Times = 100;

            private static List<SimplePoco> GenerateArray()
            {
                var random = new Random();
                var l = new List<SimplePoco>();
                for (var i = 0; i < 20000; i++)
                {
                    l.Add(new SimplePoco
                    {
                        Name = $"Bob{i}",
                        Age = random.Next(0, 1000000),
                        Nickname = Guid.NewGuid().ToString(),
                        IsHandsome = true,
                        Id = random.Next(0, 1000000)
                    });
                }

                return l;
            }

            private readonly IonSerializer _serializer = new IonSerializer();

//            [Benchmark]
            public int JsonDotnet()
            {
                var s = JsonConvert.SerializeObject(Data);
                return Encoding.UTF8.GetBytes(s).Length;
            }

//            [Benchmark]
            public int IonDotnet()
            {
                var b = IonSerializer.Serialize(Data);
                return b.Length;
            }

            private static readonly IIonWriter Writer = new ManagedBinaryWriter(IonConstants.EmptySymbolTablesArray);

            [Benchmark]
            public void IonDotnetManual()
            {
                //                _serializer.Serialize(Data);
                //                using (var stream = new MemoryStream())
                //                {
                Writer.StepIn(IonType.List);
                foreach (var poco in Data)
                {
                    Writer.StepIn(IonType.Struct);

                    Writer.SetFieldName("Age");
                    Writer.WriteInt(poco.Age);
                    Writer.SetFieldName("Name");
                    Writer.WriteString(poco.Name);
                    Writer.SetFieldName("Nickname");
                    Writer.WriteString(poco.Nickname);
                    Writer.SetFieldName("IsHandsome");
                    Writer.WriteBool(poco.IsHandsome);
                    Writer.SetFieldName("Id");
                    Writer.WriteInt(poco.Id);

                    Writer.StepOut();
                }

                Writer.StepOut();
                //                        writer.Finish(stream);

                //                }
            }
        }

        public void Run(string[] args)
        {
//            BenchmarkRunner.Run<Benchmark>();

            var bm = new Benchmark();
            var js = bm.JsonDotnet();
            var io = bm.IonDotnet();
            Console.WriteLine(js);
            Console.WriteLine(io);
            Console.WriteLine((js * 1.0) / io);
        }
    }
}
