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
            server.Subscribe(new WriteToConsoleObserver());
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
            var rpmPct = value.Sled.IsRaceOn == 1f ? value.Sled.CurrentEngineRpm / value.Sled.EngineMaxRpm : 0f;
            Console.WriteLine($"RPM: {(int)value.Sled.CurrentEngineRpm} / {(int)value.Sled.EngineMaxRpm} / SHIFT: {rpmPct >= 0.8f} / GEAR: {value.CarDash.Value.Gear}");
        }
    }
}
