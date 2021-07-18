#include <math.h>
#include <Adafruit_NeoPixel.h>

#include <ESP8266WiFi.h>
#include <WiFiUdp.h>

#include "forzaDataTypes.h"

const char *ssid = "SSID HERE";
const char *password = "WIFI PASSWORD HERE";

// Forza consts
const int SledSizeBytes = 232;
const int CarDashSizeBytes = 79;
const int HorizonCarDashBytes = 92;
const int HorizonInterstitialBytes = 12;

const int SledV1TotalBytes = SledSizeBytes;
const int SledV2TotalBytes = SledSizeBytes + CarDashSizeBytes;
const int HorizonSledTotalBytes = SledSizeBytes + HorizonCarDashBytes;

const int Protocol_SledV1 = 0;
const int Protocol_SledV2 = 1;
const int Protocol_HorizonSled = 2;

// Neopixel
#define NEOPIXEL_CONTROL_PIN 15
#define COLS_ACROSS 8
#define ROWS_DOWN 4

Adafruit_NeoPixel pixels = Adafruit_NeoPixel(COLS_ACROSS * ROWS_DOWN, NEOPIXEL_CONTROL_PIN);

const uint32_t colorBlack = pixels.Color(0, 0, 0);
const uint32_t colorGreen = pixels.Color(0, 16, 4);
const uint32_t colorYellow = pixels.Color(16, 14, 0);
const uint32_t colorRed = pixels.Color(16, 0, 0);
const uint32_t colorRedline = pixels.Color(20, 5, 0);
const uint32_t colorBlue = pixels.Color(0, 9, 16);

// UDP stuff
WiFiUDP Udp;
unsigned int localListenPort = 1337;
char incomingBuffer[SledSizeBytes + HorizonCarDashBytes];   // Support up to Horizon data packets

// Configurable redline
const short HalfColumn = COLS_ACROSS / 2;
const short BlinkFrames = 30;
const float RedlinePercentage = 0.8f;
const float DownshiftPercentage = 0.5f;
short blinkCounter = 0;
bool isBlinkOn = false;

int DetermineProtocol(int packetSize) {
  switch (packetSize) {
    case SledV1TotalBytes:
      return Protocol_SledV1;

    case SledV2TotalBytes:
      return Protocol_SledV2;

    case HorizonSledTotalBytes:
      return Protocol_HorizonSled;
  }

  return Protocol_SledV1;
}

// Note: height is 1-based
void drawColumn(int columnIndex, int height, uint32_t color) {
  for (int row = 1; row < height; ++row) {
    int index = (COLS_ACROSS * (ROWS_DOWN - row)) + columnIndex;
    pixels.setPixelColor(index, color);
  }
}

void drawTopBarLeft(uint32_t color, int length) {
  for (int col = 0; col < length; ++col) {
    pixels.setPixelColor(col, color);
  }
}

void drawTopBarRight(uint32_t color, int length) {
  for (int col = HalfColumn; col < HalfColumn + length; ++col) {
    pixels.setPixelColor(col, color);
  }
}

void updateBlinkTimer() {
  ++blinkCounter;
  if (blinkCounter >= BlinkFrames) {
    isBlinkOn = !isBlinkOn;
    blinkCounter = 0;
  }
}

void setPixelsToNumber(int numberIndex, int number) {
  int startPixelIndex = 0;
  uint32_t color = colorGreen;
  // index 0 -> starts at 0
  if (numberIndex == 1) {
    startPixelIndex = 11;
    color = colorYellow;
  }
  else if (numberIndex == 2) {
    startPixelIndex = 21;
    color = colorRed;
  }

  for (int i = startPixelIndex; i < startPixelIndex + number; ++i) {
    pixels.setPixelColor(i, color);
  }
}

void showOctet(int octet) {  
  int digit1 = octet / 100;
  int digit2 = (octet - digit1 * 100) / 10;
  int digit3 = (octet - ((digit1 * 100) + (digit2 * 10)));
  
  pixels.begin();
  setPixelsToNumber(0, digit1);
  setPixelsToNumber(1, digit2);
  setPixelsToNumber(2, digit3);
  pixels.show();
}

void updateRpm(SledData *sled, CarDashData *carDash) {
  float currentRpmPct = sled->IsRaceOn ? sled->CurrentEngineRpm / sled->EngineMaxRpm : 0;

  // Clear all pixels first. Note that this will cause some flickering
  // as the pixels are cleared, one day I'll come back and make this less annoying.
  pixels.begin();
  pixels.fill(colorBlack, 0, COLS_ACROSS * ROWS_DOWN);
  pixels.show();

  // Translate rpm percentage into visual state
  int meterLength = ceil(currentRpmPct * COLS_ACROSS);

  pixels.begin();
  // Always draw the RPM meter
  for (int i = 0; i < meterLength; ++i) {
    if (i <= 2) {
      drawColumn(i, 2, colorGreen);
    }
    else if (i == 3) {
      drawColumn(i, 3, colorGreen);
    }
    else if (i <= 5) {
      drawColumn(i, 3, colorYellow);
    }
    else {
      drawColumn(i, 4, colorRed);
    }
  }

  // Draw shift up notifier
  if (currentRpmPct >= RedlinePercentage) {
    updateBlinkTimer();

    if (isBlinkOn) {
      drawTopBarRight(colorRedline, 4);
    }
  }
  // Draw down shift notifier
  else if (currentRpmPct <= DownshiftPercentage && carDash->Gear > 1) {
    updateBlinkTimer();

    if (isBlinkOn) {
      drawTopBarLeft(colorBlue, 3);
    }
  }
  
  pixels.show();
}

void setup() {
  // Connect to wifi, stole this from esp8266 quick start guide
  Serial.begin(115200);
  Serial.println();

  Serial.printf("Connecting to %s", ssid);
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("OK");

  Udp.begin(localListenPort);
  Serial.printf("Listening at %s, UDP port: %d\n", WiFi.localIP().toString().c_str(), localListenPort);

  // Show the last octet on the display
  int octet = WiFi.localIP()[3];
  showOctet(octet);
}

void loop() {
  int packetSize = Udp.parsePacket();
  if (packetSize) {
    // Determine Forza protocol
    int protocol = DetermineProtocol(packetSize);
    
    Serial.printf("Received %d bytes from %s, port %d\n", packetSize, Udp.remoteIP().toString().c_str(), Udp.remotePort());
    int readLen = Udp.read(incomingBuffer, packetSize);
    if (readLen > 0) {
      char* pBuffer = (char*)&incomingBuffer;

      SledData *sled = (SledData *)pBuffer;
      CarDashData *carDash = NULL;

      if (protocol == Protocol_SledV2) {
        pBuffer += SledSizeBytes;
        carDash = (CarDashData *)pBuffer;
      }
      else if (protocol == Protocol_HorizonSled) {
        // Horizon (FH4 at least) has 12 mystery bytes after the initial SledV1 data, skip those for now
        pBuffer += SledSizeBytes + HorizonInterstitialBytes;
        carDash = (CarDashData *)pBuffer;
      }

      // Update the RPM display after data read
      updateRpm(sled, carDash);

      //Serial.printf("RPM: %d / %d (%.1f), Gear: %d", (int)sled->CurrentEngineRpm, (int)sled->EngineMaxRpm, currentRpmPct, (int)carDash->Gear);
    }
  }
}
