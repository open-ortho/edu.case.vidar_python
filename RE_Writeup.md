# Reverse Engineering Writeup for VIDAR Radiographic Film Scanner

## Goal
Recreate the drivers for the VIDAR Radiographic Film Scanner to be later refined to the Case Western Reserve University Bolton Center's needs. Future improvements
include a more intuitive interface, the ability to operate over a network connection, and operating a cluster of scanners at once. At this point, we are looking for a CLI to operate the scanner, which can later be
operated over SSH or as a library.

## Current Software
The current software VIDARscanNDTPRO is an antiquated .NET interface that is distributed by Vidar Systems Corporation (pictured below). From a UI/UX standpoint, it looks
pretty rough. The current interface is confusing and only allows for the operation of one scanner at a  time. There are a few essential functions that the new drivers will
have to implement: initialization, film ejection, calibration, and scanning.

![image](https://github.com/user-attachments/assets/40bd7533-97d1-4f7d-955e-4d8dfea862a4)

## Information Gathering
I started with the lowest-level interface possible and decided to move up from there. This means our starting point is Wireshark for USB packet capture.
After downloading the current software, I used Zadig to see that the USB driver used for the scanner was WinDriver6 (pictured below).

![image](https://github.com/user-attachments/assets/39b42619-bacf-4cc9-b074-1ecffdeb9c7b)

Here is an example of the USB packet capture when I initialize the calibration sequence on the scanner (Note that the capture is started after the app launches and the scanner is initialized).

![image](https://github.com/user-attachments/assets/a29f4929-f474-41b4-abac-0dff0d9ab905)

There are three groups of commands: three SCSI commands are sent to the scanner to start the calibration process. Note that for the calibration process of the scanner, there is 
no digital output; I can hear mechanical movement within the machine. Calibration was the shortest sequence of packets, so I figured it would be best to start reverse engineering.

## Reverse Engineering Calibration Packet Traffic

I used pyUSB to send packets, which meant I needed to use Zadig to change the USB interface drivers to libusbK. This led to an annoying back-and-forth of having to
uninstall and reinstall USB drivers whenever I wanted to use the old software. However, after reconstructing the SCSI command sequence (WRITE(10), SEND DIAGNOSTIC, RECIEVE DIAGNOSTIC)
I was able to achieve the calibration behavior using a Python script. 

Although this was progress, I soon came to realize a looming problem. The calibration sequence was minuscule compared to the other scanner operations. For reference, here are the Wireshark
statistics for the scanner initialization process:

![image](https://github.com/user-attachments/assets/25453439-e578-4d2d-8e05-21254096fe2e)

Almost 1000 packets to reverse engineer by hand! In reality, it is likely closer to 400 SCSI commands that need to be encoded and 400 responses that need to be decoded, but still,
that is beyond feasible to do by hand. Not to mention all of the data being exchanged, which I have no idea what it means. This was the point where I decided to move to a higher level,
I was hoping I could abstract away the packet crafting by exploiting the functions used by the application.

## Static Analysis

I moved my focus from packet capture to looking deeper into the old software to see if anything could be reused.

After opening `Vscsi32.dll` and `VIDARScanNDTPRO.exe` in Ghidra, I started by browsing found functions.

![image](https://github.com/user-attachments/assets/7737178a-a500-46d9-8904-ae8afb19f05f)

There were a good bit of reasonably high-level scanner functions that I was looking to implement. I was hopeful that I would be able to call these functions instead of recreating them myself. Looking at the decompiled C, it was not straightforward what each function was doing. This is likely due to the fairly low-level nature of this program (being drivers), here is an example:

```C

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

short initScanner(void)

{
  short sVar1;
  undefined4 uVar2;
  bool bVar3;
  
                    /* 0x72f0  106  initScanner */
  if ((DAT_1004b10a == '\x01') && (DAT_1004b10c == 1)) {
    _fprintf(DAT_10032e14,s__initScanner_1002cf28);
  }
  if (DAT_1014c3c5 == '\0') {
    return 4;
  }
  if (DAT_1014c3c3 != '\0') {
    return 8;
  }
  sVar1 = doTestUnitReady();
  if (sVar1 == -1) {
    checkCondition();
    uVar2 = getError(&DAT_10032e7c);
    if ((short)uVar2 != 0) {
      _DAT_1004b120 = (ushort)DAT_10032e7e * 0x100 + (ushort)DAT_10032e7f;
      if (_DAT_1004b120 == 0x401) {
        return 0x401;
      }
      if (_DAT_1004b120 == 0x800) {
        return 0x800;
      }
    }
  }
  FUN_10002fc0();
  DAT_1004b11c = doSetWindow(&DAT_10032db0,DAT_1002c44d);
  if (DAT_1004b11c != 0) {
    checkCondition();
    uVar2 = getError(&DAT_10032e7c);
    if ((short)uVar2 != 0) {
      _DAT_1004b120 = (ushort)DAT_10032e7e * 0x100 + (ushort)DAT_10032e7f;
      if (_DAT_1004b120 < 0x2603) {
        if (0x25ff < _DAT_1004b120) {
          return _DAT_1004b120;
        }
        bVar3 = _DAT_1004b120 == 0x1a00;
      }
      else {
        bVar3 = _DAT_1004b120 == 0x3700;
      }
      if (bVar3) {
        return _DAT_1004b120;
      }
    }
  }
  return 0;
}
```

A frustrating pattern we will continue to run into is a whole lot of pointer arithmetic and weird packing/typing. I do not know enough about decompilers or scanner drivers to say what this is due to. It was hard to infer how to call these library functions, so I needed more clues.

Looking through the headers for `VIDARScanNDTPRO.exe` led me to believe it was a `.NET` application:

![image](https://github.com/user-attachments/assets/9e09604c-54b2-49ea-b348-0c8f9625c27a)

Thinking this app was `.NET` led me to try a different decompiler: ILSpy, which is made specifically to decompile .NET applications. With a more in-depth compiler, I could see how the program makes the backend calls to replicate it.
