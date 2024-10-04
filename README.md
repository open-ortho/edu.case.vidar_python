# Python for Vidar Scanner

An attempt to reverse engineer the USB protocol used to control the Vidar Dosimetry Pro scanner, in order to be able to control it directly from the BFD-9000 tool to acquire images.

## Format

It seems like the Vidar Scanner operates over USB, but the protocol used is SCSI. Since these scanners have been around for a while, it is very likely they were once SCSI, and then they moved to the USB at a hardware level, and kept the SCSI software, which makes sense.

## Collecting data to analyze

This is how i collected packet to perform the Vidar Info operation.

1. Download [drivers from Vidar](http://www.vidar.com/film/device-drivers-for-windows-8-32-and-64-bit.htm)
2. Install on Windows 10 or earlier, or in compatibility mode.
3. Install Wireshark and USBpcap. 
4. Wireshark might require you to copy the USBpcapCMD file into its extcap directory. Follow instructions, they are pretty simple.
5. Connect Scanner, turn on, and start the Vidar Info app.
6. Start Wireshark, and select the USB interface. Then tool around with Wireshark, until you find how to disable capturing from all devices and selecting the Vidar Scanner only.
7. Start capturing packets.
8. Start the Vidar Info app.
9. Wait until it returns data from the scanner.

The Vidar Scanner has a Vidar Info

## C Code

C code in `vidar_c`. Tried to see if it was easier in C, but i don't think it is.

