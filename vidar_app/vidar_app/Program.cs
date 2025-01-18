using vidar_app;

VscsiTypes._DIGITIZERINFO digitizerInfo = new VscsiTypes._DIGITIZERINFO();
int status = Startup.InitializeDigitizer(ref digitizerInfo);

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

    default:
        Console.WriteLine("Unknown Command");
        break;
}

Console.WriteLine("");
Console.WriteLine("Shutting Down");
