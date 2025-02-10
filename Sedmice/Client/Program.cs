using Klase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
                //sad kad smo se uspesno konektovali poslacemo nase ime da bi sinhronizovali
                //tj primamo prvo poruku za sinhronizaciju i onda cemo da odgovorimo sa imenom
                while(true)
                {
                    if(tcpSocket.Poll(1000*500,SelectMode.SelectRead))
                    {
                        int bytesRecived = tcpSocket.Receive(buffer);
                        using(MemoryStream ms = new MemoryStream(buffer))
                        {
                            paket = (Paket)bf.Deserialize(ms);
                        }
                        if(paket.syn == true)
                        {
                            paket.message = igrac.Ime;
                            using(MemoryStream ms = new MemoryStream())
                            {
                                bf.Serialize(ms, paket);
                                databuffer = ms.ToArray();
                            }
                            tcpSocket.Send(databuffer);
                            break;
                        }
                    }
                }
                Console.WriteLine("Nase ime " + igrac.Ime + " " + tcpSocket.LocalEndPoint.ToString());

                //krece igra
                Console.Clear();//brisemo konzolu zbog pocetka igre
                while (true)
                {
                    Paket pkt = new Paket();
                    while (Console.KeyAvailable)
                    {
                        Console.ReadKey(true); //da slucajno ne bi neko pritiskao dok nije njihhov red i to se odma racunalo u njihov potez
                    }
                    if (tcpSocket.Poll(1000*500,SelectMode.SelectRead))
                    {
                        Console.Clear();
                        int byteRecived = tcpSocket.Receive(buffer);
                        using(MemoryStream ms = new MemoryStream(buffer))
                        {
                            pkt = (Paket)bf.Deserialize(ms);
                        }
                        if(pkt.igra == true)
                        {
                            igrac = pkt.igrac;
                            igrac.IspisiKarte();
                            Console.Write("Sto: ");
                            IspisiSto(pkt.stanjeStola);
                            ConsoleColor org = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine(pkt.message);
                            Console.ForegroundColor = org;
                            if (pkt.potez == true)
                            {
                                int kartaZaIgranje;
                                do
                                {
                                    Console.WriteLine("Vi ste na redu,izaberite kartu koju zelite da odigrate ");
                                    kartaZaIgranje = Int32.Parse(Console.ReadLine());
                                    if (kartaZaIgranje == 5)
                                        kartaZaIgranje = 0;
                                }
                                while (kartaZaIgranje > igrac.KarteURuciCount || kartaZaIgranje < 0);
                                kartaZaIgranje = kartaZaIgranje - 1;
                                Paket paket1 = new Paket();
                                paket1.igra = true;
                                paket1.kartaZaIgranje = kartaZaIgranje;
                                using(MemoryStream ms = new MemoryStream())
                                {
                                    bf.Serialize(ms, paket1);
                                    databuffer = ms.ToArray();
                                }
                                tcpSocket.Send(databuffer);
                                pkt = new Paket();
                            }
                        }
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }
        public static void IspisiSto(List<Karta> sto)
        {
            ConsoleColor orgBoja = Console.ForegroundColor;
            foreach (Karta karta in sto)
            {
                if (karta.Znak == Znak.Herc || karta.Znak == Znak.Karo)
                    Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(karta.ToString() + ", ");
                Console.ForegroundColor = orgBoja;
            }
            Console.WriteLine();
        }
    }
}
