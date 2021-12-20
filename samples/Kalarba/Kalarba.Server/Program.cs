using System;
using Kalarba.Server;
using Microsoft.Owin.Hosting;

const string address = "http://localhost:58779/";
using (WebApp.Start<Startup>(address))
{
    Console.WriteLine($"Server is running on {address}, press CTRL+C to stop.");
    Console.ReadLine();
}
