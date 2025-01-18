using vidar_app;

int status = Startup.InitializeDigitizer();

Console.WriteLine("Enter a command: ");
string command = Console.ReadLine().ToLower();

switch(command)
{
    case "calibrate":
        Calibrate.calibrate();
        break;

    default:
        Console.WriteLine("Unknown Command");
        break;
}

Console.WriteLine("");
Console.WriteLine("Shutting Down");