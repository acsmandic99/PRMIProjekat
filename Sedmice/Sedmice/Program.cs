using Klase;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace Sedmice
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string ime = "", prezime = "";
            int tim;
            Console.WriteLine("Upisite Vase ime: ");
            ime = Console.ReadLine();
            Console.WriteLine("Upisite Vase prezime: ");
            prezime = Console.ReadLine();
            Console.WriteLine("Upisite Vas tim(broj 1 ili 2): ");
            tim = Int32.Parse(Console.ReadLine());
            Igrac igrac = new Igrac(tim, prezime, ime);
            Console.WriteLine("Upisite IP adresu servera na koju zelite da se povezete: ");
            string[] adresa = Console.ReadLine().Split(':');
            string IPAdresa = adresa[0];
            int port = Int32.Parse(adresa[1]);

            Socket udpSocket = new Socket(AddressFamily.InterNetwork,SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Blocking = false;
            EndPoint udpEP = new IPEndPoint(IPAddress.Any, 0);
            udpSocket.Bind(udpEP);

            Paket paket;
            EndPoint serverUDPEP = new IPEndPoint(IPAddress.Parse(IPAdresa),port);
            byte[] buffer = new byte[4096];
#pragma warning disable SYSLIB0011 // Type or member is obsolete izbacivalo mi error,al ne znam sto nije mi izbacivalo u serveru
            BinaryFormatter bf = new BinaryFormatter();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
            int tcpPort;
            try
            {
                using(MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, igrac);
                    buffer = ms.ToArray();
                }
                udpSocket.SendTo(buffer, serverUDPEP);
                while(true)
                {
                    if(udpSocket.Poll(1000*500,SelectMode.SelectRead))
                    {
                        int bytesRecived = udpSocket.ReceiveFrom(buffer, SocketFlags.None, ref serverUDPEP);
                        using(MemoryStream ms = new MemoryStream())
                        {
                            paket = (Paket)bf.Deserialize(ms);
                            if(paket.succsess == false)
                            {
                                Console.WriteLine(paket.message);
                                Console.WriteLine("Upisite novo ime igraca: ");
                                igrac.Ime = Console.ReadLine();
                                bf.Serialize(ms, igrac);
                                buffer = ms.ToArray();
                                udpSocket.SendTo(buffer, serverUDPEP);
                            }
                            else
                            {
                                tcpPort = paket.port;
                                Console.WriteLine(paket.message);
                            }
                        }
                    }
                }

            }
            catch(SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }
    }
}
