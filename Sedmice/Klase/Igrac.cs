using System;

namespace Klase
{
	[Serializable]
    public class Igrac
    {
		private string ime = "Anonimac";
		private string prezime = "Anonimni";
		private int brojPoena = 0;
		private int tim;

        public Igrac(int tim, string prezime, string ime)
        {
            Tim = tim;
            Prezime = prezime;
            Ime = ime;
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
			set { Ime = value; }
		}

	}
}
