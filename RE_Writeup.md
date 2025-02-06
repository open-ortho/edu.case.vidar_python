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
