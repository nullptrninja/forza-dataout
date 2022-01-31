# Forza-Dataout
A DataOut library for various applications with Forza Motorsport and Forza Horizon titles.

## Supported Titles
- Forza Motorsport 7
- Forza Horizon 4
- Forza Horizon 5

## What is this?
This repo contains a few components that you can use for your own projects:
- A C# UDP listen server that receives, parses, and publishes each UDP packet to a listener of your own implementation
- An Arduino project if you wanted to build your own shift light. See the [arduino_forzaShiftLight](https://github.com/nullptrninja/forza-dataout/tree/main/arduino_forzaShiftLight) directory for detailed build instructions.

## C# Project Components
The C# projected located under the [/src](https://github.com/nullptrninja/forza-dataout/tree/main/src) directory contains 2 components:
- Core
- ConsoleOutputListener

### Core Project
The Core project provides the base UDP parsing and pub/sub functionality. To use it, you'll need to instantiate a `UdpListener` object and pass in a few arguments:

```
var config = new ForzaTelemetryConfig() {
    ListenPort = 1337
};

var server = new UdpListener<ForzaDataModel>(config, new ForzaGenericConverter());
```

The `ForzaTelemetryConfig` just provides the port that your server will listen to. Additional settings can be added into that object as needed. The `ForzaGenericConverter` object is what performs the UDP datagram parsing. In this case the "generic" object provides parsing for FM7 and FH4 Sled V1 and V2 data formats. Future Forza titles will add new converters as needed.

### ConsoleOutputListener
This is a "sample" and/or debug component + driver that receives events from the UDP server and writes them out to console. You can look at the `Program.cs` file for additional details on usage.
To subscribe a listener (in this case the `ConsoleOutputListener`) to the server you can use the following line:

```
var unsubscriber = server.Subscribe(new WriteToConsoleObserver());
```

You can use the `unsubscriber` reference to dynamically remove this listener from the server's publish targets.

### Notes
This is a rough implementation to get the basics working but doesn't handle error scenarios (though I haven't observed any edge cases coming out of Forza telemetry yet).
