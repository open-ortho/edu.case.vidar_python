using vidar_app;

VscsiTypes._DIGITIZERINFO? scannerInfo = Startup.InitializeDigitizer();

if (!scannerInfo.HasValue)
{
    return;
} else
{
    VscsiTypes._DIGITIZERINFO digitizerInfo = scannerInfo.Value;

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
}