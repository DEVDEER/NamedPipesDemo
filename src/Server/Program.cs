using System.IO.Pipes;
using System.Runtime.Intrinsics.Arm;
using System.Text;

using Logic;

await StartServerAsync();

static string GetHash(string original, Encoding encoding)
{
    var bytes = encoding.GetBytes(original);
    var hashedBytes = System.IO.Hashing.Crc32.Hash(bytes);
    return encoding.GetString(hashedBytes);
}

static async Task StartServerAsync()
{
    var threadId = Thread.CurrentThread.ManagedThreadId;
    Console.WriteLine($"Starting server-pipe 'testpipe' on thread {threadId}");
    using var pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.InOut, 4);
    Console.WriteLine("Started. Waiting for clients...");
    // Wait for a client to connect
    await pipeServer.WaitForConnectionAsync();
    Console.WriteLine("Client connected on thread[{0}].", threadId);
    try
    {
        // Read the request from the client. Once the client has
        // written to the pipe its security token will be available.
        var ss = new StreamString(pipeServer);
        // Verify our identity to the connected client using a
        // string that the client anticipates.
        var code = ss.ReadString();
        if (code != "0815")
        {
            pipeServer.Close();
            return;
        }
        Console.WriteLine("Client authenticated.");
        while (true)
        {
            var ticks = DateTime.Now.Ticks.ToString("0000000000000000000");
            var x = "012200";
            var y = "090100";
            var a = "02122";
            var data = $"{ticks}{x}{y}{a}11";
            // calculate the checksum
            var checkSum = GetHash(data, Encoding.ASCII); 
            var payload = $"{data}{checkSum}";
            ss.WriteStringEx(payload);
            await Task.Delay(10);
        }
    }
    // Catch the IOException that is raised if the pipe is broken
    // or disconnected.
    catch (IOException e)
    {
        Console.WriteLine("ERROR: {0}", e.Message);
    }
    pipeServer.Close();
}