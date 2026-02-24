<img src="./documentation/images/BFD9000_logo_white.png" alt="BFD9010" width="300">

# BFD9010: An HL7 FHIR API for Scanners

This tools was originally created to control the Vidar Dosimetry Pro scanner for the BFD-9000 project. However, it has been repurposed so that it can be easily re-used with any scanner by just writing scanner drivers for it.


This software reverse-engineers the USB protocol to enable direct control of the scanner.

## Quick Start

### For End Users (Windows)

1. **Download** the latest release from the releases page
2. **Run** `bfd9010.exe` (GUI application)
   - The scanner will initialize automatically
   - A small window will appear showing status
   - The FHIR API server starts on `http://localhost:5000`
3. **Navigate** to `https://wingate.case.edu/bfd9000/` to scan
4. The web application will communicate with your local scanner via the API

### For Developers

See the [API README](BFD9010/API_README.md) for detailed documentation on:
- Building from source
- FHIR API endpoints
- Integration details
- Architecture overview

## Components

### 1. bfd9010 (CLI)
Command-line interface for direct scanner control.
```bash
cd BFD9010.Cli/bin/Release/net8.0
./bfd9010.exe
```

### 2. bfd9010 (GUI + API Server)
Windows Forms application that:
- Shows scanner status in always-on-top window
- Hosts FHIR REST API on port 5000
- Initializes scanner automatically

### 3. BFD9010.Scanner (Library)
Shared library containing all scanner operations:
- Scanner initialization and discovery
- Scan operations
- Calibration
- Film ejection
- Configuration management

### 4. BFD9010.FhirApi (API Server)
Standalone FHIR REST API server (can run without GUI).

## FHIR API Endpoints

- `GET /Device/{id}` - Get scanner information
- `POST /Device/{id}/$scan` - Perform scan, returns PNG image as base64 in FHIR Bundle
- `POST /Device/{id}/$calibrate` - Calibrate scanner
- `POST /Device/{id}/$eject` - Eject film

See [FHIR Device Resource](https://hl7.org/fhir/device.html) for detailed documentation of the FHIR standard for the Device.

See [API README](BFD9010/API_README.md) for detailed documentation.

## Project History

This is an attempt to reverse engineer the USB protocol used to control the Vidar Dosimetry Pro scanner, in order to be able to control it directly from the BFD-9000 tool to acquire images.

## Technical Background

### USB Protocol Format

It seems like the Vidar Scanner operates over USB, but the protocol used is SCSI. Since these scanners have been around for a while, it is very likely they were once SCSI, and then they moved to the USB at a hardware level, and kept the SCSI software, which makes sense.

### Collecting data to analyze

This is how packet capture was performed for the Vidar Info operation:

1. Download [drivers from Vidar](http://www.vidar.com/film/device-drivers-for-windows-8-32-and-64-bit.htm)
2. Install on Windows 10 or earlier, or in compatibility mode.
3. Install Wireshark and USBpcap. 
4. Wireshark might require you to copy the USBpcapCMD file into its extcap directory. Follow instructions, they are pretty simple.
5. Connect Scanner, turn on, and start the Vidar Info app.
6. Start Wireshark, and select the USB interface. Then tool around with Wireshark, until you find how to disable capturing from all devices and selecting the Vidar Scanner only.
7. Start capturing packets.
8. Start the Vidar Info app.
9. Wait until it returns data from the scanner.

For more details on the reverse engineering process, see [RE_Writeup.md](BFD9010/RE_Writeup.md).

## Legacy Code

### .NET Code

.NET code in `BFD9010.Cli`. This is a console app that uses the `Vscsi32.dll` library to control the scanner. This is the most functional code so far.

## Format

It seems like the Vidar Scanner operates over USB, but the protocol used is SCSI. Since these scanners have been around for a while, it is very likely they were once SCSI, and then they moved to the USB at a hardware level, and kept the SCSI software, which makes sense.
