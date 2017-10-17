using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodiceFiscaleUtility;

namespace CodiceFiscaleCmdLine
{
    class Program
    {
        static void Main(string[] args)
        {
            CodiceFiscale cf = null;
            Console.WriteLine("1) Genera codice fiscale");
            Console.WriteLine("2) Ottieni dati da codice fiscale");
            Console.Write("Scelta > ");
            ConsoleKeyInfo input = Console.ReadKey();
            Console.Write("\n");
            if (input.KeyChar == '1')
            {
                Console.Write("Cognome > ");
                string Cognome = Console.ReadLine();
                Console.Write("Nome > ");
                string Nome = Console.ReadLine();
                Console.Write("Sesso > ");
                string Sesso = Console.ReadLine();
                Console.Write("Nascita > ");
                DateTime Nascita = Convert.ToDateTime(Console.ReadLine());
                Console.Write("Comune > ");
                string Comune = Console.ReadLine();
                Console.Write("Provincia > ");
                string Provincia = Console.ReadLine();
                Console.Write("Livello di omocodia > ");
                int LivelloOmocodia = Int32.Parse(Console.ReadLine());
                cf = new CodiceFiscale(Cognome, Nome, Sesso, Nascita, Comune, Provincia, LivelloOmocodia);
            }
            else if (input.KeyChar == '2')
            {
                Console.Write("Inserisci il codice fiscale > ");
                cf = new CodiceFiscale(Console.ReadLine());
            }
            foreach (var prop in cf.GetType().GetProperties())
            {
                Console.WriteLine("{0}={1}", prop.Name, prop.GetValue(cf, null));
            }
            Console.WriteLine("Premi un tasto per uscire...");
            Console.ReadKey();
        }
    }
}
