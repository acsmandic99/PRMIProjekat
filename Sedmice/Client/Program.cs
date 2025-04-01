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
            byte[] buffer = new byte[8192];
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
                Paket paketTest = new Paket();
                bool nova = false;
                while (true)
                {
                    while (Console.KeyAvailable)
                    {
                        Console.ReadKey(true); //da slucajno ne bi neko pritiskao dok nije njihhov red i to se odma racunalo u njihov potez
                    }
                    if (tcpSocket.Poll(1000*100,SelectMode.SelectRead))
                    {
                        Paket pkt = new Paket();
                        Console.Clear();
                        int byteRecived = tcpSocket.Receive(buffer);
                        using(MemoryStream ms = new MemoryStream(buffer))
                        {
                            pkt = (Paket)bf.Deserialize(ms);
                        }
                        paketTest = pkt;
                        igrac = pkt.igrac;
                        Console.WriteLine("Ime " + igrac.Ime);
                        Console.WriteLine("Karte u ruci: ");
                        igrac.IspisiKarte();
                        Console.Write("Sto: ");
                        IspisiSto(pkt.stanjeStola);
                        Console.WriteLine(pkt.message);
                        if (pkt.stanjeIgre == StanjeIgre.REDOVNA_IGRA)
                        {
                            if (pkt.NaPotezu.Equals(igrac.Ime))
                            {
                                Console.WriteLine("Vi ste na potezu,izaberite Vasu kartu!");
                                do
                                {
                                    pkt.kartaZaIgranje = Int32.Parse(Console.ReadLine());
                                } while (pkt.kartaZaIgranje > igrac.KarteURuciCount || pkt.kartaZaIgranje < 1);
                                pkt.kartaZaIgranje = pkt.kartaZaIgranje - 1;
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    bf.Serialize(ms, pkt);
                                    databuffer = ms.ToArray();
                                }
                                tcpSocket.Send(databuffer);
                            }
                        }
                        else if (pkt.stanjeIgre == StanjeIgre.ODGOVOR_PRVOG_IGRACA && pkt.NaPotezu.Equals(igrac.Ime))
                        {
                            Console.WriteLine(pkt.specijalnaPoruka);
                            Console.WriteLine("Vi ste na potezu,izaberite Vasu kartu!");
                            do
                            {
                                pkt.kartaZaIgranje = Int32.Parse(Console.ReadLine());
                                if (pkt.kartaZaIgranje == 5)
                                    pkt.kartaZaIgranje = 0;
                            } while (pkt.kartaZaIgranje > igrac.KarteURuciCount || pkt.kartaZaIgranje < 0);
                            pkt.kartaZaIgranje = pkt.kartaZaIgranje - 1;
                            using (MemoryStream ms = new MemoryStream())
                            {
                                bf.Serialize(ms, pkt);
                                databuffer = ms.ToArray();
                            }
                            tcpSocket.Send(databuffer);
                        }
                        else if (pkt.stanjeIgre == StanjeIgre.ZAVRSETAK_IGRE && nova == false)
                        {
                            do
                            {
                                Console.WriteLine("Odgovorite sa Y-DA ili N - NE");
                                pkt.novaIgra = Console.ReadLine().ToString()[0];
                            } while (pkt.novaIgra.ToString().ToUpper().Equals("Y") && pkt.novaIgra.ToString().ToUpper().Equals("N"));
                            using (MemoryStream ms = new MemoryStream())
                            {
                                bf.Serialize(ms, pkt);
                                databuffer = ms.ToArray();
                            }
                            tcpSocket.Send(databuffer);
                            Console.WriteLine("Cekaju se ostali igraci");
                            nova = true;
                        }
                    }
                }
            }
            catch (SocketException ex)
            {
                udpSocket.Close();
                Console.WriteLine(ex.Message);
            }

            udpSocket.Close();
            
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
