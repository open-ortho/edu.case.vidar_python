# Reverse Engineering Writeup for VIDAR Radiographic Film Scanner

## Contents
- [Goal](#goal)
- [Current Software](#current-software)
- [Information Gathering](#information-gathering)
- [Reverse Engineering Calibration Packet Traffic](#reverse-engineering-calibration-packet-traffic)
- [Static Analysis](#static-analysis)
- [What do we Really Need?](#what-do-we-really-need)
  

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

Looking through the headers for `VIDARScanNDTPRO.exe` led me to believe it was a .NET application:

![image](https://github.com/user-attachments/assets/9e09604c-54b2-49ea-b348-0c8f9625c27a)

Thinking this app was .NET led me to try a different decompiler: ILSpy, which is made specifically to decompile .NET applications. With a more in-depth compiler, I could see how the program makes the backend calls to replicate it.

Using ILSpy means I can see the .NET (C#) code for `VIDARScanNDTPRO.exe`:

![image](https://github.com/user-attachments/assets/72d8ea3e-a18f-4c0f-b47b-0ada577bfdca)

The .NET application makes calls to the DLL (`Vscsi32.dll`) for low-level calls.

Here is a simple example from the `DigitizerEngine` that calls the DLL's `Calibrate()` function.

```C#
	public void Normalize()
	{
		DigitizeEngine digitizeEngine = null;
		DigitizeEngine digitizeEngine2 = new DigitizeEngine();
		try
		{
			digitizeEngine = digitizeEngine2;
			ErrorCode = global::<Module>.Calibrate();
			if (ErrorCode != 0)
			{
				CheckError();
			}
		}
		catch
		{
			//try-fault
			((IDisposable)digitizeEngine).Dispose();
			throw;
		}
		((IDisposable)digitizeEngine).Dispose();
	}
```

In this case, calling the `Calibrate()` function is fairly straightforward as it takes no parameters and clearly returns an error code; however, this is not always the case.

As a more complex example, here is a call to the `ScanFilm()` function:

```C#

*(int*)(&sCANFILM) = (int)(&num2);
System.Runtime.CompilerServices.Unsafe.As<_SCANFILM, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref sCANFILM, 16)) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gETDIGINFO, 4));
System.Runtime.CompilerServices.Unsafe.As<_SCANFILM, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref sCANFILM, 12)) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::<Module>.?A0x8f06ef6e.sp);
System.Runtime.CompilerServices.Unsafe.As<_SCANFILM, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref sCANFILM, 4)) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::<Module>.?A0x8f06ef6e.image_buffer);
System.Runtime.CompilerServices.Unsafe.As<_SCANFILM, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref sCANFILM, 8)) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::<Module>.?A0x8f06ef6e.total_bytes_received);
System.Runtime.CompilerServices.Unsafe.As<_SCANPARAMETERS, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref global::<Module>.?A0x8f06ef6e.sp, 52)) = 0;
global::<Module>.GetTickCount();
uint tickCount = global::<Module>.GetTickCount();
void* ptr3 = (void*)(int)global::<Module>._beginthreadex(null, 0u, (delegate* unmanaged[Stdcall, Stdcall]<void*, uint>)global::<Module>.__unep@?ScanFilm@?A0x8f06ef6e@@$$FYGIPAX@Z, &sCANFILM, 0u, null);
``` 

The call is on that last line, called with parameters, the most notable of which is `sCANFILM`, which is populated above with some other values that need to be determined. `sCANFILM` seems to be a structure that contains some fields, one of which is a pointer to a `_SCANPARAMETERS` structure, which itself is a collection of fields. Here is where we gain another level of complexity as we do not get the decompilation of these structs; we can just see their total size in memory.

For example, here is the `_SCANFILM` struct:

```C#
[StructLayout(LayoutKind.Sequential, Size = 20)]
[DebugInfoInPDB]
[MiscellaneousBits(65)]
[NativeCppClass]
internal struct _SCANFILM
{
}
```

We know that a `_SCANFILM` instance is 20 bytes in total, but we need to infer anything else based upon how it is used. Here is the population of an instance of `_SCANFILM`:

```		C#
*(int*)(&sCANFILM) = (int)(&num2);
System.Runtime.CompilerServices.Unsafe.As<_SCANFILM, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref sCANFILM, 16)) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gETDIGINFO, 4));
System.Runtime.CompilerServices.Unsafe.As<_SCANFILM, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref sCANFILM, 12)) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::<Module>.?A0x8f06ef6e.sp);
System.Runtime.CompilerServices.Unsafe.As<_SCANFILM, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref sCANFILM, 4)) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::<Module>.?A0x8f06ef6e.image_buffer);
System.Runtime.CompilerServices.Unsafe.As<_SCANFILM, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref sCANFILM, 8)) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::<Module>.?A0x8f06ef6e.total_bytes_received);
```

For the sake of readability, I will remove some of the decompilation artifacts and substitute some pseudocode:

```C#
// The first field is inferred to be a pointer to "num2"
*(int*)(&sCANFILM) = (int)(&num2);

// The fifth field is a pointer to the value stored at byte position 4 in "gETDIGINFO" (this is scanner initialization info)
sCANFILM[16] = (int)gETDIGINFO[4];

// The fourth field is a pointer to a "sp" (this is a scan parameters struct)
sCANFILM[12] = (int)global::<Module>.?A0x8f06ef6e.sp;

// The second field, a pointer, is the buffer to which the scanned data is written.
sCANFILM[4] = (int)global::<Module>.?A0x8f06ef6e.image_buffer;

// The third field is a pointer to a variable tracking the current total bytes recieved from the scanner
sCANFILM[8] = (int)global::<Module>.?A0x8f06ef6e.total_bytes_received;
```

Now we get a clearer picture of what the `_SCANFILM` struct stores and how.

This is what most of the work of this reverse engineering looked like: searching the decompilation and inferring internal properties after clearing out all of the garbage.

## What do we Really Need?

The next task was to narrow down what functions from the `vscsi32.dll` library I needed to use for basic scanner functions. This is where we take our first step into dynamic analysis. Using x32dbg, we can connect to a process and see all available and called functions.
After connecting to a running instance of `VIDARScannerNDTPRO.exe`, we can see all of the imported functions available from `vscsi32.dll`:

![image](https://github.com/user-attachments/assets/95ad8a4c-9855-480e-b560-c1c338f83793)

It is important to note that there are many functions that seem to implement basic actions as helper procedures. It would be more useful to see what the scanner software (`VIDARScannerNDTPRO.exe`) is directly calling.
Setting some breakpoints and poking around with the software led to this list of directly called functions: `findDigitizer`, `getDigitizerInfo`, `EjectFilm`, `Calibrate`, and `Scan`. Here is the call counter after starting up the program, ejecting a film, and calibrating the scanner:

![image](https://github.com/user-attachments/assets/d425ea3a-f1e7-4d69-af8e-660bbb586f32)

Looking at the timing and hits counter in the dynamic debugger, we can infer what these functions are doing. `EjectFilm`, `Calibrate`, and `Scan` are exactly what you expect; they do the respective actions. `findDigitizer` is called once during startup; it searches the computer's USB interfaces for the scanner hardware device. The `getDigitizerInfo` is called on startup, and before each of the three actions, it queries the connected scanner's available settings and other information.

Now, we have a list of five functions we want our new drivers to be able to call. Next is the actual implementation.



