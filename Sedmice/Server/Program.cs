using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Klase;


namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Spil spil = new Spil();
            spil.Ispisi();
            spil.Promesaj();
            spil.Ispisi();
            Karta k;
            int size = spil.Count();
            for(int i = 0;i<size;i++)
            {
                k = spil.IzvuciKartu();
                Console.WriteLine("Izvukli ste " + k.ToString());
            }
            spil.Ispisi();
            Console.ReadLine();
        }
    }
}
