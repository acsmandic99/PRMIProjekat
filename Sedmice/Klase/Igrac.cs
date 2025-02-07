using System;
using System.Collections.Generic;

namespace Klase
{
	[Serializable]
    public class Igrac
    {
		private string ime = "Anonimac";
		private string prezime = "Anonimni";
		private int brojPoena = 0;
		private int tim;
		private List<Karta> karteURuci = new List<Karta>();
		private List<Karta> osvojeneKarte = new List<Karta>();

        public Igrac(int tim, string prezime, string ime)
        {
            Tim = tim;
            Prezime = prezime;
            Ime = ime;
        }

		public Karta OdigrajKartu(int index)
		{
			Karta karta = karteURuci[index - 1];
			karteURuci.RemoveAt(index - 1);
			return karta;
		}
		public void IspisiKarte()
		{
			int i = 1;
			foreach(Karta karta in  karteURuci)
			{
				Console.WriteLine(i + " - " + karta.ToString());
				i++;
			}
		}
		public void IzvuciKartu(Karta novaKarta)
		{
			karteURuci.Add(novaKarta);
		}

		public void NosiKarte(Karta osvojenaKarta)
		{
			osvojeneKarte.Add(osvojenaKarta);
		}
		public int IzbrojOsvojenePoene()
		{
			int poeni = 0;
			foreach(Karta karta in osvojeneKarte)
			{
				if (karta.Broj == VrednostKarte.Deset || karta.Broj == VrednostKarte.A)
					poeni++;
			}
			return poeni;
		}
        public int Tim
		{
			get { return tim; }
			set { tim = value; }
		}


		public int BrojPoena
		{
			get { return brojPoena; }
			set { brojPoena = value; }
		}


		public string Prezime
		{
			get { return prezime; }
			set { prezime = value; }
		}


		public string  Ime
		{
			get { return ime; }
			set { ime = value; }
		}

	}
}
