using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klase
{
    [Serializable]
	public enum Znak { Tref,Pik,Karo,Herc}
    public class Karta
    {
		private int broj;
		private Znak znak;

		public Znak Znak
		{
			get { return znak; }
			set { znak = value; }
		}

		public int Broj
		{
			get { return broj; }
			set { broj = value; }
		}

	}
}
