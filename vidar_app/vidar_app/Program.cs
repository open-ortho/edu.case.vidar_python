using vidar_app;
using static vidar_app.Scanner;

ScannerData scanner_data = new ScannerData();
VscsiTypes._DIGITIZERINFO digitizerInfo = new VscsiTypes._DIGITIZERINFO();

//parses readable data out of returned DIGITIZERINFO, prints it and returns a populated scanner_data struct.
int status = Startup.InitializeDigitizer(ref digitizerInfo, ref scanner_data);

if (status != 0)
{
    return;
}

Console.WriteLine("Enter a command: ");
string command = Console.ReadLine().ToLower();

switch (command)
{
    case "calibrate":
        Calibrate.calibrate();
        break;

    case "eject":
        Eject.eject(digitizerInfo);
        break;

    case "scan":
        Scan.scan(digitizerInfo, scanner_data);
        break;

    default:
        Console.WriteLine("Unknown Command");
        break;
}

Console.WriteLine("");
Console.WriteLine("Shutting Down");