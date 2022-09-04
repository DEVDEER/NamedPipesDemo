// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Principal;

using Logic;

if (args.Length > 0)
{
    if (args[0] == "spawnclient")
    {
        var pipeClient = new NamedPipeClientStream(
            ".",
            "testpipe",
            PipeDirection.InOut,
            PipeOptions.None,
            TokenImpersonationLevel.Impersonation);
        Console.WriteLine("Connecting to server...\n");
        await pipeClient.ConnectAsync();
        var ss = new StreamString(pipeClient);
        // Validate the server's signature string.
        await Task.Delay(1000);
        ss.WriteString("0815");
        while (true)
        {
            var payload = ss.ReadString();
            Console.WriteLine(payload);
        }
        Console.WriteLine("Server closed connection. Wait until session is terminated.");
        pipeClient.Close();
        // Give the client process some time to display results before exiting.
        Thread.Sleep(4000);
    }
}
else
{
    Console.WriteLine("\n*** Named pipe client stream with impersonation example ***\n");
    StartClients();
}

// Helper function to create pipe client processes
static void StartClients()
{
    var currentProcessName = Environment.CommandLine;

    // Remove extra characters when launched from Visual Studio
    currentProcessName = currentProcessName.Trim('"', ' ');
    currentProcessName = Path.ChangeExtension(currentProcessName, ".exe");
    var plist = new Process[4];
    Console.WriteLine("Spawning client processes...\n");
    if (currentProcessName.Contains(Environment.CurrentDirectory))
    {
        currentProcessName = currentProcessName.Replace(Environment.CurrentDirectory, string.Empty);
    }

    // Remove extra characters when launched from Visual Studio
    currentProcessName = currentProcessName.Replace("\\", string.Empty);
    currentProcessName = currentProcessName.Replace("\"", string.Empty);
    int i;
    for (i = 0; i < 4; i++)
    {
        // Start 'this' program but spawn a named pipe client.
        plist[i] = Process.Start(currentProcessName, "spawnclient");
    }
    while (i > 0)
    {
        for (var j = 0; j < 4; j++)
        {
            if (plist[j] != null)
            {
                if (plist[j]
                    .HasExited)
                {
                    Console.WriteLine($"Client process[{plist[j].Id}] has exited.");
                    plist[j] = null;
                    i--; // decrement the process watch count
                }
                else
                {
                    Thread.Sleep(250);
                }
            }
        }
    }
    Console.WriteLine("\nClient processes finished, exiting.");
}