# Deployment Requirements

This document lists deployment prerequisites and system requirements for running BFD9010 on Windows.

## Hardware
- Windows 10/11 with 32-bit support (x86)
- Vidar Pro scanner (or compatible model)
- USB connection to the scanner

## Software
- .NET 8.0 Runtime
- Vidar scanner driver stack (32-bit)
  - Driver installation requires administrator rights
  - The driver must be installed before the scanner is detected

## Operational Requirements
- Scanner powered on and connected via USB
- No other Vidar software running (including another BFD9010 instance)
- Port 5000 available for the local API server
- Windows Firewall allows the application to accept local connections

## See Also
- Installation and usage: `documentation/BUILD_AND_RUN.md`
- API endpoints and testing: `documentation/API.md`
