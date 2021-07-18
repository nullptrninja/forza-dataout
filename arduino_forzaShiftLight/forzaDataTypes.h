#ifndef __FORZADATATYPES_H__
#define __FORZADATATYPES_H__

typedef struct SledData_t {
  // = 1 when race is on. = 0 when in menus/race stopped …
  int IsRaceOn;

  //Can overflow to 0 eventually
  unsigned int TimestampMS;

  float EngineMaxRpm;
  float EngineIdleRpm;
  float CurrentEngineRpm;

  //In the car's local space; X = right, Y = up, Z = forward
  float AccelerationX;
  float AccelerationY;
  float AccelerationZ;

  //In the car's local space; X = right, Y = up, Z = forward
  float VelocityX;
  float VelocityY;
  float VelocityZ;

  //In the car's local space; X = pitch, Y = yaw, Z = roll
  float AngularVelocityX;
  float AngularVelocityY;
  float AngularVelocityZ;

  float Yaw;
  float Pitch;
  float Roll;

  // Suspension travel normalized: 0.0f = max stretch; 1.0 = max compression
  float NormalizedSuspensionTravelFrontLeft;
  float NormalizedSuspensionTravelFrontRight;
  float NormalizedSuspensionTravelRearLeft;
  float NormalizedSuspensionTravelRearRight;

  // Tire normalized slip ratio, = 0 means 100% grip and |ratio| > 1.0 means loss of grip.
  float TireSlipRatioFrontLeft;
  float TireSlipRatioFrontRight;
  float TireSlipRatioRearLeft;
  float TireSlipRatioRearRight;

  // Wheel rotation speed radians/sec.
  float WheelRotationSpeedFrontLeft;
  float WheelRotationSpeedFrontRight;
  float WheelRotationSpeedRearLeft;
  float WheelRotationSpeedRearRight;

  // = 1 when wheel is on rumble strip, = 0 when off.
  int WheelOnRumbleStripFrontLeft;
  int WheelOnRumbleStripFrontRight;
  int WheelOnRumbleStripRearLeft;
  int WheelOnRumbleStripRearRight;

  // = from 0 to 1, where 1 is the deepest puddle
  float WheelInPuddleDepthFrontLeft;
  float WheelInPuddleDepthFrontRight;
  float WheelInPuddleDepthRearLeft;
  float WheelInPuddleDepthRearRight;

  // Non-dimensional surface rumble values passed to controller force feedback
  float SurfaceRumbleFrontLeft;
  float SurfaceRumbleFrontRight;
  float SurfaceRumbleRearLeft;
  float SurfaceRumbleRearRight;

  // Tire normalized slip angle, = 0 means 100% grip and |angle| > 1.0 means loss of grip.
  float TireSlipAngleFrontLeft;
  float TireSlipAngleFrontRight;
  float TireSlipAngleRearLeft;
  float TireSlipAngleRearRight;

  // Tire normalized combined slip, = 0 means 100% grip and |slip| > 1.0 means loss of grip.
  float TireCombinedSlipFrontLeft;
  float TireCombinedSlipFrontRight;
  float TireCombinedSlipRearLeft;
  float TireCombinedSlipRearRight;

  // Actual suspension travel in meters
  float SuspensionTravelMetersFrontLeft;
  float SuspensionTravelMetersFrontRight;
  float SuspensionTravelMetersRearLeft;
  float SuspensionTravelMetersRearRight;

  // Unique ID of the car make/model
  int CarOrdinal;

  // Between 0 (D -- worst cars) and 7 (X class -- best cars) inclusive 
  int CarClass;

  // Between 100 (slowest car) and 999 (fastest car) inclusive
  int CarPerformanceIndex;

  // Corresponds to EDrivetrainType; 0 = FWD, 1 = RWD, 2 = AWD
  int DrivetrainType;

  // Number of cylinders in the engine
  int NumCylinders;
} SledData;

typedef struct CarDashData_t {
  // Position (meters)
  float PositionX;
  float PositionY;
  float PositionZ;
  
  float Speed; // meters per second
  float Power; // watts
  float Torque; // newton meter
  
  float TireTempFrontLeft;
  float TireTempFrontRight;
  float TireTempRearLeft;
  float TireTempRearRight;
  
  float Boost;
  float Fuel;
  float DistanceTraveled;
  float BestLap;
  float LastLap;
  float CurrentLap;
  float CurrentRaceTime;
  
  unsigned short LapNumber;
  byte RacePosition;
  
  byte Accel;
  byte Brake;
  byte Clutch;
  byte HandBrake;
  byte Gear;
  signed char Steer;
  
  signed char NormalizedDrivingLine;
  signed char NormalizedAIBrakeDifference;
  // Note the last missing byte for WORD alignment
} CarDashData;

#endif
