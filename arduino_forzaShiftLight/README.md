# Arduino-based Shift Light
This project implements a compact shift light for all the diehards that prefer to row their own gears in Forza. We implement this using 2 specific components:
- [Adafruit Feather HUZZAH32 ESP32](https://www.adafruit.com/product/3405) - Arduino-based microcontroller
- [Adafruit NeoPixel FeatherWing](https://www.adafruit.com/product/2945) - A matrix display of 32 addressable RGB LEDs

Since this only requires 2 parts, it's very easy to assemble yourself with no need for wires or additional resistors. Just 5 minutes of soldering and you should be good to go!

# Build steps
1. The HUZZAH board should have the female stacking headers in order to support the FeatherWing, so if yours does not have it then your first step is to get them installed to your Feather. You can opt to pay a little extra to get them pre-assembled for you if you'd like.

![image](https://user-images.githubusercontent.com/26495597/126060027-0b0cde91-cef1-4901-8f0e-40faa4cee78f.png)

2. On the NeoPixel FeatherWing's underside, you will see a series of jumper pads with one pad looking different than the others on PIN #6; it looks like 2 squares:

![image](https://user-images.githubusercontent.com/26495597/126058928-c0469b56-d1d7-4114-a33c-c97cb62c0970.png)

You will need to find a sharp razor blade (a box cutter can work as well if the blade is thin enough) - and use the blade to cut the thin metal bridge between the 2 square pads on PIN #6:

![image](https://user-images.githubusercontent.com/26495597/126059022-ab17ea83-1cf7-4f76-8ae1-f71be0adca7c.png)

If you have a multimeter, use the continuity tester and probe the upper square pad and lower square pad to ensure the jumper is no longer connected.

3. You will now need to solder closed the jumper pads on PIN #15:

![image](https://user-images.githubusercontent.com/26495597/126059099-3d0d723f-34ab-48f6-afc4-c3980c200ce8.png)

It should look like this (probably better than my solder job):

![image](https://user-images.githubusercontent.com/26495597/126059620-157312fa-e152-4e8c-bbf4-e6b2cefc4c89.png)

4. Use the continuity tester on your multimeter to ensure these 2 pads are now bridged together.
5. Solder on the male stacking pins on the NeoPixel FeatherWing. These pins should've been provided with your FeatherWing.
6. Attach the NeoPixel FeatherWing on top of your HUZZAH32 Feather board.
7. Attach a USB cable from your PC to the HUZZAH32 Feather board and load the Arduino IDE
8. If you need to set up your IDE to use Adafruit boards, look at these [setup instructions](https://learn.adafruit.com/adafruit-huzzah32-esp32-feather/using-with-arduino-ide) to make sure the appropriate libraries are installed.
9. Open the `arduino_forzaShiftLight.ino` file in this directory into the IDE.
10. Find the 2 lines in the INO that ask you for your router's SSID and PASSWORD and fill them in. You can also find the `localListenPort` variable and change that if needed (defaults to 1337).
11. Deploy the code to your Feather.

# Configuration
Once the code is deployed to the Feather board, you may connect it to a power source. After a few seconds the device will acquire an IP address from your network. If successful you'll see the NeoPixels light up in a specific pattern:

![image](https://user-images.githubusercontent.com/26495597/126059502-e4ce635d-f7c9-4e5a-b920-4171e37714dd.png)

This pattern represents the fourth octet in the acquired IP address. Simply count the number of lit pixels per color to get each digit's value in the octet.
The first color (teal) is the first digit, the second color (yellow) is the second digit, and the third color (red) is the third digit. In ths above example this is `182`
We only give the last octet because we make the assumption that the first 3 octets are already known to you (usually `192.168.1.xxx`). Make a note of the number you see here because this goes away once it starts receiving data from Forza.

Start up Forza (in this example we're using FH4) and navigate to the following:
- "CONTINUE" (or "NEW GAME" if you haven't started one up yet).
- Once you're in the world, hit ESC and click on SETTINGS
 
![image](https://user-images.githubusercontent.com/26495597/126059654-0f99f666-f5dc-4400-925e-b32bda78d035.png)
- Go to "HUD AND GAMEPLAY"
 
![image](https://user-images.githubusercontent.com/26495597/126059669-87ab2518-8b7d-40b9-aeb7-4d10d039db62.png)
- Scroll down until you see the "DATA OUT" option and toggle it to "ON"
- Configure the IP and PORT to match the IP address and port (default 1337 if you didn't change it) of the Arduino/Feather

![image](https://user-images.githubusercontent.com/26495597/126059705-b8b9c058-0c20-49e2-92b8-a43c124c01b8.png)

- Save the settings and you're done! Once you're back in the open world (where you can freely drive around) you will see the LEDs light up as your RPMs change:

![image](https://user-images.githubusercontent.com/26495597/126059737-b3edbd05-256b-41b2-90f8-9c1ff1c143f7.png)

- When your revs hit the shift point you will see a blinking red bar on the top-right edge of the display:

![image](https://user-images.githubusercontent.com/26495597/126059763-433d73e0-13e3-4df3-9a6b-3701b6eeffbd.png)

- When your revs are too low for your selected gear, you will see a blinking blue bar on the top-left edge of the display indicating that you need to downshift.

## About "Redline"
If you look at the "Sled" data format in [forzaDataTypes.h](https://github.com/nullptrninja/forza-dataout/blob/main/arduino_forzaShiftLight/forzaDataTypes.h#L11) you'll notice that none of the fields contain any indication as to what the car's actual redline point is. As a result, the device itself will never know the real redline, so therefore we make an averaged guess of where most redlines will be.
Looking at the [INO file](https://github.com/nullptrninja/forza-dataout/blob/main/arduino_forzaShiftLight/arduino_forzaShiftLight.ino#L48) we have 2 variables that control when our program will signal a gear change is needed:
- `RedlinePercentage` denotes at which percentage of the rev range (relative to MaxEngineRpm) will the upshift blinker turn on. We default this to 80% but we've observed that super-low revving engines (like some muscle cars) will throw this way off.
- `DownshiftPercentage` denotes at which percentage of the rev range will the downshift blinker turn on. This defaults to 50% and is generally on the conservative side to avoid annoying you if your revs aren't spot on for the current gear.

You can adjust these 2 variables to your liking based on where you prefer to shift as well as what you typically drive.
