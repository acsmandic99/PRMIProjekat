using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using Start;

namespace Sedmice
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Unesite broj klijenata:");
            int count = Int32.Parse(Console.ReadLine());


            new PokreniKlijente().PokreniKlijente1(count);

        }

        
    }
}
