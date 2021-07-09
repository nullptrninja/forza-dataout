using Core;
using Core.Converters;
using Core.Models;
using System;

namespace ConsoleOutputListener {
    class Program {
        static void Main(string[] args) {
            var config = new ForzaTelemetryConfig() {
                ListenPort = 1337
            };

            var server = new UdpListener<ForzaDataModel>(config, new ForzaGenericConverter());
            var unsubHandle = server.Subscribe(new WriteToConsoleObserver());
            Console.WriteLine($"Listening on UDP Port: {config.ListenPort}");
            server.Start();
        }
    }

    class WriteToConsoleObserver : IObserver<ForzaDataModel> {
        public void OnCompleted() {
            Console.WriteLine("Finished listening");
        }

        public void OnError(Exception error) {
            Console.WriteLine("Error occurred");
        }

        public void OnNext(ForzaDataModel value) {
            if (value.Sled.IsRaceOn == 1) {
                Console.WriteLine($"RPM: {value.Sled.CurrentEngineRpm}/{value.Sled.EngineMaxRpm} GEAR: {value.CarDash.Value.Gear}");
            }
        }
    }
}
