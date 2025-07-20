using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start
{
    public class PokreniKlijente
    {
        public void PokreniKlijente1(int brojKlijenata)
        {
            for (int i = 0; i < brojKlijenata; i++)
            {
                // Putanja do izvršnog fajla klijenta (potrebno je kompajlirati ga)  
                string clientPath = @"C:\Users\Nikola\Documents\GitHub\PRMIProjekat\Sedmice\Client\bin\Debug\Client.exe";
                Process klijentProces = new Process(); // Stvaranje novog procesa 
                klijentProces.StartInfo.FileName = clientPath; //Zadavanje putanje za pokretanje 
                klijentProces.StartInfo.Arguments = $"{i + 2}"; // Argument - broj klijenta  
                klijentProces.Start(); // Pokretanje klijenta  
                Console.WriteLine($"Pokrenut klijent #{i + 2}");
            }
        }
    }
}
