using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Klase
{
    public class Igra
    {
        Igrac[] igraci;
        Socket[] igraciSocket;
        Spil spil = new Spil();
        List<Karta> sto = new List<Karta>();
        static BinaryFormatter bf = new BinaryFormatter();
        static byte[] buffer = new byte[4096];
        int igracNaPotezu = 0;
        int prviIgrac;
        bool prvi_potez = true;
        public Igra(Igrac[] igraci, Socket[] igraciSocket)
        {
            this.igraci = igraci;
            this.igraciSocket = igraciSocket;
        }

        public void Igraj()
        {
            spil.Promesaj();
            DodeliKarteIgracima();
            Paket paket = new Paket();
            paket.message = "Na potezu je igrac: " + igraci[igracNaPotezu].Ime;
            prviIgrac = igracNaPotezu;
            PosaljiIgracimaStanje(paket);
            bool udaren = false;
            int potezCounter = 0;
            while(true)
            {
                for(int i = 0;i<igraci.Length;i++)
                {
                    if (igraciSocket[i].Poll(1000*100,SelectMode.SelectRead))
                    {
                        Console.WriteLine("Primljena poruka od igraca na mestu " + i);
                        if(i == igracNaPotezu)
                        {
                            Paket paket1 = new Paket();
                            int bytesRecived = igraciSocket[i].Receive(buffer);
                            using(MemoryStream ms = new MemoryStream(buffer))
                            {
                                paket1 = (Paket)bf.Deserialize(ms);
                            }
                            if(prvi_potez == true && sto.Count>0 && i == prviIgrac)
                            {
                                //znaci da je neko udario donju kartu i da je napravljen pun krug i da je prvi igrac
                                //imao isti znak koji zeli da udari opet
                                //tu proveravamo da li je bacio dobru kartu
                                if(paket1.kartaZaIgranje != -1)
                                {
                                    Karta zeljenaKarta = igraci[i].KartaNaIndexu(paket1.kartaZaIgranje);
                                    if (zeljenaKarta.Broj != sto[0].Broj)
                                    {
                                        //bacio je pogresnu kartu moramo da mu javimo da mora da izabere korektnu kartu!
                                        Paket p = new Paket();
                                        p.igra = true;
                                        p.potez = true;
                                        p.stanjeStola = sto;
                                        p.igrac = igraci[i];
                                        p.message = "5 - Ne odigraj nista - Zavrsi krug\nMolim Vas morate da odigrate znak kao prva karta bacena.\nAko nemate kartu sa tim znakom odigrajte opciju 5 - zavrsite krug!";
                                        byte[] dataBuffer;
                                        using(MemoryStream ms = new MemoryStream())
                                        {
                                            bf.Serialize(ms,p);
                                            dataBuffer = ms.ToArray();
                                        }
                                        igraciSocket[i].Send(dataBuffer);
                                        break;
                                    }
                                }
                                else
                                {
                                    ZapocniNoviKrug();
                                    break;
                                }
                            }

                            Karta odigranaKarta = igraci[i].OdigrajKartu(paket1.kartaZaIgranje);
                            Console.WriteLine("Igrac " + igraci[i].Ime + " je odigrao " + odigranaKarta.ToString());
                            Paket paketZaSlanje = new Paket();
                            paketZaSlanje.igra = true;
                            
                            sto.Add(odigranaKarta);
                            if(sto.Count > 1)
                            {
                                if (odigranaKarta.Broj == sto[0].Broj)
                                    udaren = true;
                            }
                            igracNaPotezu++;
                            if (igracNaPotezu == igraci.Length)
                                igracNaPotezu = 0;

                            paketZaSlanje.message = "";
                            if (sto.Count % igraci.Length == prviIgrac && sto.Count > 0 && udaren == true)
                            {
                                //znaci zavrsio se krug i sledeci igrac je opet igrac koji je zapoceo krug
                                Console.WriteLine("Zavrsio se prvi krug,sad pocinje drugi krug");
                                paketZaSlanje.message += "5 - Ne odigraj nista - Zavrsi krug\n";
                                prvi_potez = true;
                                potezCounter = 0;
                            }
                            else if(sto.Count % igraci.Length == prviIgrac && sto.Count > 0 && udaren == false)
                            {
                                ZapocniNoviKrug();
                                break;
                            }
                            paketZaSlanje.message += "Igrac " + igraci[i].Ime + " je odigrao " + odigranaKarta.ToString();
                            paketZaSlanje.message += "\nNa potezu je igrac: " + igraci[igracNaPotezu].Ime;
                            Console.WriteLine("Sledeci igrac na potezu je " + igraci[igracNaPotezu].Ime);
                            if(potezCounter > 0)
                                prvi_potez = false;
                            potezCounter++;
                            PosaljiIgracimaStanje(paketZaSlanje);
                            
                            break;
                        }
                    }
                }
            }
        }

        public int OdrediKoNosiKarte()
        {
            int index = prviIgrac;
            for (int i = 0; i < sto.Count; i++)
            {
                if (sto[i].Znak == sto[0].Znak)
                {
                    index = i % igraci.Length;
                }
            }
            return index;
        }
        public void ZapocniNoviKrug()
        {
            int nosi_karte = OdrediKoNosiKarte();
            foreach (Karta karta in sto)
            {
                igraci[nosi_karte].NosiKarte(karta);
            }
            sto.Clear();
            prvi_potez = true;
            igracNaPotezu = nosi_karte;
            Paket paketNoviKrug = new Paket();
            paketNoviKrug.message = "Igrac " + igraci[nosi_karte].Ime + " je osvojio sto.\nNa potezu je igrac: " + igraci[igracNaPotezu].Ime;
            DodeliKarteIgracima();
            PosaljiIgracimaStanje(paketNoviKrug);
        }
        public void DodeliKarteIgracima()
        {
            int i = igracNaPotezu;
            while (igraci[i % igraci.Length].KarteURuciCount < 4)
            {
                Karta karta = spil.IzvuciKartu();
                igraci[i % igraci.Length].IzvuciKartu(karta);
                i++;
            }
        }
        public void PosaljiIgracimaStanje(Paket paket)
        {
            
            for (int i = 0;i<igraci.Length;i++)
            {
                byte[] dataBuffer;
                paket.igra = true;

                paket.stanjeStola = sto;
                paket.igrac = igraci[i];
                if (i == igracNaPotezu)
                {
                    paket.potez = true;
                }
                else
                {
                    paket.potez = false;
                }
                using(MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, paket);
                    dataBuffer = ms.ToArray();
                }
                igraciSocket[i].Send(dataBuffer);
            }
        }

        public string ispisiSto()
        {
            ConsoleColor orgBoja = Console.ForegroundColor;
            string retVal = "Sto:\n";
            for(int i = 0;i<sto.Count;i++)
            {
                retVal += sto[i].ToString() + "   ";
            }

            return retVal;
        }
    }
}
