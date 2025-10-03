using vidar_app;
using static vidar_app.Scanner;

Console.WriteLine("Looking for Scanner...");

ScannerData scanner_data = new ScannerData();
VscsiTypes._DIGITIZERINFO digitizerInfo = new VscsiTypes._DIGITIZERINFO();

//parses readable data out of returned DIGITIZERINFO, prints it and returns a populated scanner_data struct.
int status = Startup.InitializeDigitizer(ref digitizerInfo, ref scanner_data);

if (status != 0)
{
    return;
}

Console.WriteLine("\nWelcome to Vidar Scanner Console!");
Console.WriteLine("-------------------------------------------------------------");
Console.WriteLine("Available commands (press the first letter):");
Console.WriteLine("  [C]alibrate  - Calibrate the digitizer");
Console.WriteLine("  [E]ject      - Eject the film from the digitizer");
Console.WriteLine("  [S]can       - Initiate a scan using the digitizer");
Console.WriteLine("-------------------------------------------------------------");

Console.Write("Enter a command: ");
string command = Console.ReadLine().Trim().ToLower();

switch (command)
{
    case "c":
        Calibrate.calibrate();
        break;

    case "e":
        Eject.eject(digitizerInfo);
        break;

    case "s":
        Scan.scan(digitizerInfo, scanner_data);
        break;

    default:
        Console.WriteLine("Unknown Command");
        break;
}

Console.WriteLine("");
Console.WriteLine("Shutting Down");