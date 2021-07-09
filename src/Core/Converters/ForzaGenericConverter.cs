using Core.Contracts;
using Core.Models;
using System.IO;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Core.Converters {
	// A generic forza data converter. This converter should work for the following titles:
	// Forza Motorsport 7
	// Forza Horizon 4
    public class ForzaGenericConverter : IDataModelConverter<ForzaDataModel> {
        private static readonly int SledBytes = Marshal.SizeOf(typeof(SledData));
        private static readonly int CarDashBytes = Marshal.SizeOf(typeof(CarDashData));
		private static readonly int HorizonCarDashBytes = 92;		// FH4 doesn't document its CarDash data model, this is just the difference other users
																	// found between FH4 datagrams minus SledBytes from FM7. The size will cover at least the CarDashData structure.

		private static readonly int FH4PaddingBytes = 12;			// There's a 12 byte offset after the Sled in FH4. We can ignore this data for our purposes.

		// The byte length of the entire datagram determines which protocol we use when reading
        private static readonly int ProtocolSledOnlySizeBytes = SledBytes;
        private static readonly int ProtocolSledAndCarDashSizeBytes = SledBytes + CarDashBytes;
        private static readonly int ProtocolSledAndHorizonCarDashSizeBytes = SledBytes + HorizonCarDashBytes;

        public ForzaDataModel Convert(byte[] data) {
            var protocol = DetermineProtocol(data);

            var fdm = new ForzaDataModel() {
                Protocol = protocol
            };

            if (protocol == ProtocolData.Unknown) {
                return fdm;
            }

			using var memStream = new MemoryStream(data);
			using var streamReader = new BinaryReader(memStream);

			// Every protocol has sled data at least
			fdm.Sled = ReadSled(streamReader);

			// CarDash is for V2 of the data packet in addition to Sled data. This was introduced in later version of FM7
			if (protocol == ProtocolData.CarDash) {
				fdm.CarDash = ReadCarDash(streamReader);
            }
			else if (protocol == ProtocolData.HorizonCarDash) {
				fdm.CarDash = ReadHorizonCarDashData(streamReader);
            }

			return fdm;
		}

        private static ProtocolData DetermineProtocol(byte[] data) {
            if (data.Length == ProtocolSledOnlySizeBytes) {
                return ProtocolData.Sled;
            }
            else if (data.Length == ProtocolSledAndCarDashSizeBytes) {
                return ProtocolData.CarDash;
            }
            else if (data.Length == ProtocolSledAndHorizonCarDashSizeBytes) {
                return ProtocolData.HorizonCarDash;
            }

            return ProtocolData.Unknown;
        }

        private static SledData ReadSled(BinaryReader reader) {
			return new SledData {
				IsRaceOn = reader.ReadInt32(),
				TimestampMS = reader.ReadUInt32(),

				EngineMaxRpm = reader.ReadSingle(),
				EngineIdleRpm = reader.ReadSingle(),
				CurrentEngineRpm = reader.ReadSingle(),

				AccelerationX = reader.ReadSingle(),
				AccelerationY = reader.ReadSingle(),
				AccelerationZ = reader.ReadSingle(),

				VelocityX = reader.ReadSingle(),
				VelocityY = reader.ReadSingle(),
				VelocityZ = reader.ReadSingle(),

				AngularVelocityX = reader.ReadSingle(),
				AngularVelocityY = reader.ReadSingle(),
				AngularVelocityZ = reader.ReadSingle(),

				Yaw = reader.ReadSingle(),
				Pitch = reader.ReadSingle(),
				Roll = reader.ReadSingle(),

				NormalizedSuspensionTravelFrontLeft = reader.ReadSingle(),
				NormalizedSuspensionTravelFrontRight = reader.ReadSingle(),
				NormalizedSuspensionTravelRearLeft = reader.ReadSingle(),
				NormalizedSuspensionTravelRearRight = reader.ReadSingle(),

				TireSlipRatioFrontLeft = reader.ReadSingle(),
				TireSlipRatioFrontRight = reader.ReadSingle(),
				TireSlipRatioRearLeft = reader.ReadSingle(),
				TireSlipRatioRearRight = reader.ReadSingle(),

				WheelRotationSpeedFrontLeft = reader.ReadSingle(),
				WheelRotationSpeedFrontRight = reader.ReadSingle(),
				WheelRotationSpeedRearLeft = reader.ReadSingle(),
				WheelRotationSpeedRearRight = reader.ReadSingle(),

				WheelOnRumbleStripFrontLeft = reader.ReadInt32(),
				WheelOnRumbleStripFrontRight = reader.ReadInt32(),
				WheelOnRumbleStripRearLeft = reader.ReadInt32(),
				WheelOnRumbleStripRearRight = reader.ReadInt32(),

				WheelInPuddleDepthFrontLeft = reader.ReadSingle(),
				WheelInPuddleDepthFrontRight = reader.ReadSingle(),
				WheelInPuddleDepthRearLeft = reader.ReadSingle(),
				WheelInPuddleDepthRearRight = reader.ReadSingle(),

				SurfaceRumbleFrontLeft = reader.ReadSingle(),
				SurfaceRumbleFrontRight = reader.ReadSingle(),
				SurfaceRumbleRearLeft = reader.ReadSingle(),
				SurfaceRumbleRearRight = reader.ReadSingle(),

				TireSlipAngleFrontLeft = reader.ReadSingle(),
				TireSlipAngleFrontRight = reader.ReadSingle(),
				TireSlipAngleRearLeft = reader.ReadSingle(),
				TireSlipAngleRearRight = reader.ReadSingle(),

				TireCombinedSlipFrontLeft = reader.ReadSingle(),
				TireCombinedSlipFrontRight = reader.ReadSingle(),
				TireCombinedSlipRearLeft = reader.ReadSingle(),
				TireCombinedSlipRearRight = reader.ReadSingle(),

				SuspensionTravelMetersFrontLeft = reader.ReadSingle(),
				SuspensionTravelMetersFrontRight = reader.ReadSingle(),
				SuspensionTravelMetersRearLeft = reader.ReadSingle(),
				SuspensionTravelMetersRearRight = reader.ReadSingle(),

				CarOrdinal = reader.ReadInt32(),
				CarClass = reader.ReadInt32(),
				CarPerformanceIndex = reader.ReadInt32(),
				DrivetrainType = reader.ReadInt32(),
				NumCylinders = reader.ReadInt32()
			};
		}

		private static CarDashData ReadCarDash(BinaryReader reader) {
			return new CarDashData {
				PositionX = reader.ReadSingle(),
				PositionY = reader.ReadSingle(),
				PositionZ = reader.ReadSingle(),

				Speed = reader.ReadSingle(),
				Power = reader.ReadSingle(),
				Torque = reader.ReadSingle(),

				TireTempFrontLeft = reader.ReadSingle(),
				TireTempFrontRight = reader.ReadSingle(),
				TireTempRearLeft = reader.ReadSingle(),
				TireTempRearRight = reader.ReadSingle(),

				Boost = reader.ReadSingle(),
				Fuel = reader.ReadSingle(),
				DistanceTraveled = reader.ReadSingle(),
				BestLap = reader.ReadSingle(),
				LastLap = reader.ReadSingle(),
				CurrentLap = reader.ReadSingle(),
				CurrentRaceTime = reader.ReadSingle(),

				LapNumber = reader.ReadUInt16(),
				RacePosition = reader.ReadByte(),

				Accel = reader.ReadByte(),
				Brake = reader.ReadByte(),
				Clutch = reader.ReadByte(),
				HandBrake = reader.ReadByte(),
				Gear = reader.ReadByte(),
				Steer = reader.ReadSByte(),

				NormalizedDrivingLine = reader.ReadSByte(),
				NormalizedAIBrakeDifference = reader.ReadSByte()
			};
		}

		private static CarDashData ReadHorizonCarDashData(BinaryReader reader) {
			// Read the padding of unknown FH4 bytes
			reader.ReadBytes(FH4PaddingBytes);

			// We can streamline by calling ReadCarDash() but since it's a struct we would make an extra copy
			var dashData = new CarDashData {
				PositionX = reader.ReadSingle(),
				PositionY = reader.ReadSingle(),
				PositionZ = reader.ReadSingle(),

				Speed = reader.ReadSingle(),
				Power = reader.ReadSingle(),
				Torque = reader.ReadSingle(),

				TireTempFrontLeft = reader.ReadSingle(),
				TireTempFrontRight = reader.ReadSingle(),
				TireTempRearLeft = reader.ReadSingle(),
				TireTempRearRight = reader.ReadSingle(),

				Boost = reader.ReadSingle(),
				Fuel = reader.ReadSingle(),
				DistanceTraveled = reader.ReadSingle(),
				BestLap = reader.ReadSingle(),
				LastLap = reader.ReadSingle(),
				CurrentLap = reader.ReadSingle(),
				CurrentRaceTime = reader.ReadSingle(),

				LapNumber = reader.ReadUInt16(),
				RacePosition = reader.ReadByte(),

				Accel = reader.ReadByte(),
				Brake = reader.ReadByte(),
				Clutch = reader.ReadByte(),
				HandBrake = reader.ReadByte(),
				Gear = reader.ReadByte(),
				Steer = reader.ReadSByte(),

				NormalizedDrivingLine = reader.ReadSByte(),
				NormalizedAIBrakeDifference = reader.ReadSByte()
			};

			// Note there's like an extra byte remaining in FH4, we don't need this (or know what to do with it)

			return dashData;
        }
    }
}
