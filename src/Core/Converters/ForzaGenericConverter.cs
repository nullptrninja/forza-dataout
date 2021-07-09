using Core.Contracts;
using Core.Models;
using System.IO;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Core.Converters {
    public class ForzaGenericConverter : IDataModelConverter<ForzaDataModel> {
        private static readonly int SledBytes = Marshal.SizeOf(typeof(SledData));
        private static readonly int CarDashBytes = Marshal.SizeOf(typeof(CarDashData));

		// Can't seem to find any docs about what the extra 92 bytes actually contain. We'll just read it in as padding.
		private static readonly int HorizonCarDashBytes = 92;

		// The byte length of the entire datagram determines which protocol we use when reading
        private static readonly int ProtocolSledOnlySizeBytes = SledBytes;
        private static readonly int ProtocolSledAndCarDashSizeBytes = SledBytes + CarDashBytes;
        private static readonly int ProtocolSledAndHorizonCarDashSizeBytes = SledBytes + 92;

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
			var sledData = ReadSled(streamReader);

			// Sled-only is for older version of FM7 before SledV2. So if we're not Sled, we assume there's CarDash.
			CarDashData carDashData;
			if (protocol != ProtocolData.Sled) {
				carDashData = ReadCarDash(streamReader);
				fdm.CarDash = carDashData;
            }
			
			// Burn the next bytes for FH4 because we don't know what to do with it.
			if (protocol == ProtocolData.HorizonCarDash) {
				ReadHorizonCarDashData(streamReader);
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

		private static void ReadHorizonCarDashData(BinaryReader reader) {
			reader.ReadBytes(HorizonCarDashBytes);
        }
    }
}
