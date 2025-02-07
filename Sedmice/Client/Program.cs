using Klase;
using System;
using System.IO;
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

            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Blocking = false;
            EndPoint localEP = new IPEndPoint(IPAddress.Any, 0);
            udpSocket.Bind(localEP);

            EndPoint serverUDPEP = new IPEndPoint(IPAddress.Parse(IPAdresa), port);
            byte[] buffer = new byte[4096];
            byte[] databuffer;
            Paket paket;
            BinaryFormatter bf = new BinaryFormatter();
            int tcpPort;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, igrac);
                    databuffer = ms.ToArray();
                }
                udpSocket.SendTo(databuffer, serverUDPEP);
                while (true)
                {
                    if (udpSocket.Poll(1000 * 500, SelectMode.SelectRead))
                    {
                        int bytesRecived = udpSocket.ReceiveFrom(buffer, SocketFlags.None, ref serverUDPEP);
                        using (MemoryStream mems = new MemoryStream(buffer))
                        {
                            paket = (Paket)bf.Deserialize(mems);

                            if (paket.succsess == false)
                            {
                                Console.WriteLine(paket.message);
                                Console.WriteLine("Upisite novo ime igraca: ");
                                igrac.Ime = Console.ReadLine();
                                using(MemoryStream ms = new MemoryStream())
                                {
                                    bf.Serialize(ms, igrac);
                                    databuffer = ms.ToArray();
                                }
                                udpSocket.SendTo(databuffer, serverUDPEP);
                            }
                            else if(paket.succsess == true && paket.port == 0)
                            {
                                Console.WriteLine(paket.message);
                            }
                            else if(paket.succsess == true && paket.port != 0)
                            {
                                tcpPort = paket.port;
                                udpSocket.Close();
                                break;
                            }
                        }
                        
                    }
                }
                Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcpSocket.Bind(localEP);
                IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(IPAdresa), tcpPort);
                tcpSocket.Connect(serverEP);
                Console.WriteLine("Uspesno smo se konektovali na TCP server port");


            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }
    }
}
