namespace Klase
{
	[Serializable]
    public class Igrac
    {
		private	string ime;
		private string prezime;
		private int brojPoena;
		private int tim;

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
