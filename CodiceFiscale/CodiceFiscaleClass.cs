using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Text.RegularExpressions;

namespace CodiceFiscaleUtility
{
    class CodiceFiscale
    {
        private static DataSet data;
        private static string omocodiciRegex;
        private static string mesiRegex;
        private static string vocali = "AEIOU";
        private static string consonanti = "BCDFGHJKLMNPQRSTVWXYZ";
        private int[] posizioniOmocodia = { 14, 13, 12, 10, 9, 7, 6 };
        public string Codice { get; private set; }
        public string CodiceNormalizzato { get; private set; }
        public string Nome { get; private set; }
        public string Cognome { get; private set; }
        public DateTime Nascita { get; private set; }
        public string Comune { get; private set; }
        public string Provincia { get; private set; }
        public string CodiceComune { get; private set; }
        public string Sesso { get; private set; }
        public int LivelloOmocodia { get; private set; }
        
        public CodiceFiscale(string codiceFiscale) {
            string cfNoOmocodiciRegex = @"^[A-Z]{6}\d{2}" + mesiRegex + @"\d{2}[A-Z]\d{3}[A-Z]";
            string cfRegex = @"^[A-Z]{6}" + omocodiciRegex + "{2}" + mesiRegex + omocodiciRegex + "{2}[A-Z]" + omocodiciRegex + "{3}[A-Z]";
            codiceFiscale = codiceFiscale.ToUpper();
            if (!Regex.Match(codiceFiscale, cfRegex).Success)
            {
                throw new ArgumentException("Codice fiscale non valido");
            }
            if (getCIN(codiceFiscale.Substring(0,15))!=codiceFiscale.Substring(15,1))
            {
                throw new ArgumentException("Carattere di controllo non valido");
            }
            this.Codice = codiceFiscale;
            if (Regex.Match(this.Codice, cfNoOmocodiciRegex).Success)
            {
                this.CodiceNormalizzato = this.Codice;
                this.LivelloOmocodia = 0;
            }
            else
            {
                // si tratta di un omocodice
                StringBuilder cfNormalizzato = new StringBuilder(this.Codice);
                cfNormalizzato.Remove(15, 1);
                int livelloOmocodiaRagg = 0;
                foreach (int i in posizioniOmocodia)
                {
                    if (Char.IsLetter(cfNormalizzato[i]))
                    {
                        livelloOmocodiaRagg++;
                        string tmpLettera = cfNormalizzato[i].ToString();
                        cfNormalizzato.Remove(i, 1);
                        cfNormalizzato.Insert(i, ottieniOmocodice(tmpLettera));
                    }
                }
                cfNormalizzato.Append(getCIN(cfNormalizzato.ToString()));
                this.LivelloOmocodia = livelloOmocodiaRagg;
                this.CodiceNormalizzato = cfNormalizzato.ToString();
            }
            this.Cognome = this.CodiceNormalizzato.Substring(0, 3);
            this.Nome = this.CodiceNormalizzato.Substring(3, 3);
            int anno = Int32.Parse(this.CodiceNormalizzato.Substring(6, 2));
            int thisAnno = DateTime.Now.Year;
            thisAnno = thisAnno - ((thisAnno / 100) * 100);
            if (anno > thisAnno)
                anno = anno + 1900;
            else
                anno = anno + 2000;
            int mese = convertiMese(this.CodiceNormalizzato.Substring(8, 1));
            int giorno = Int32.Parse(this.CodiceNormalizzato.Substring(9, 2));
            if (giorno > 40)
            {
                this.Sesso = "F";
                giorno = giorno - 40;
            }
            else
            {
                this.Sesso = "M";
            }
            this.Nascita = new DateTime(anno, mese, giorno);
            this.CodiceComune = this.CodiceNormalizzato.Substring(11, 4);
            this.comuneProvinciaDaCodice();
        }
        public CodiceFiscale(string Cognome, string Nome, string Sesso, DateTime Nascita, string CodiceComune, int LivelloOmocodia = 0)
        {
            if (String.IsNullOrWhiteSpace(Nome))
                throw new ArgumentException("Nome non impostato");
            this.Nome = Nome;
            if (String.IsNullOrWhiteSpace(Cognome))
                throw new ArgumentException("Cognome non impostato");
            this.Cognome = Cognome;
            Sesso = Sesso.ToUpper();
            if (!Sesso.Equals("M") && !Sesso.Equals("F"))
                throw new ArgumentException("Sesso non valido");
            this.Sesso = Sesso;
            this.Nascita = Nascita;
            if (String.IsNullOrWhiteSpace(CodiceComune))
                throw new ArgumentException("Codice comune non impostato");
            this.CodiceComune = CodiceComune;
            this.comuneProvinciaDaCodice();
            if (LivelloOmocodia<0)
                throw new ArgumentException("Livello omocodia non valido");
            this.LivelloOmocodia = LivelloOmocodia;
            this.calcolaCodice();
        }
        public CodiceFiscale(string Cognome, string Nome, string Sesso, DateTime Nascita, string Comune, string Provincia, int LivelloOmocodia = 0)
        {
            if (String.IsNullOrWhiteSpace(Nome))
                throw new ArgumentException("Nome non impostato");
            this.Nome = Nome;
            if (String.IsNullOrWhiteSpace(Cognome))
                throw new ArgumentException("Cognome non impostato");
            this.Cognome = Cognome;
            Sesso = Sesso.ToUpper();
            if (!Sesso.Equals("M") && !Sesso.Equals("F"))
                throw new ArgumentException("Sesso non valido");
            this.Sesso = Sesso;
            this.Nascita = Nascita;
            if (String.IsNullOrWhiteSpace(Comune))
                throw new ArgumentException("Comune non impostato");
            this.Comune = Comune;
            if (String.IsNullOrWhiteSpace(Provincia))
                throw new ArgumentException("Provincia non impostata");
            this.Provincia = Provincia;
            this.codiceDaComuneProvincia();
            if (LivelloOmocodia < 0)
                throw new ArgumentException("Livello omocodia non valido");
            this.LivelloOmocodia = LivelloOmocodia;
            this.calcolaCodice();
        }


        private void calcolaCodice()
        {
            StringBuilder tmpCodice = new StringBuilder(16);
            tmpCodice.Append(calcolaNome(this.Cognome, isCognome: true));
            tmpCodice.Append(calcolaNome(this.Nome));
            tmpCodice.Append(this.Nascita.Year.ToString().Substring(2, 2));
            tmpCodice.Append(convertiMese(this.Nascita.Month));
            int giornoNascita = this.Nascita.Day;
            if (this.Sesso == "F")
            {
                giornoNascita += 40;
            }
            tmpCodice.Append(giornoNascita.ToString("D2"));
            tmpCodice.Append(this.CodiceComune);
            tmpCodice.Append(getCIN(tmpCodice.ToString()));
            this.CodiceNormalizzato = tmpCodice.ToString();
            if (LivelloOmocodia > 0)
            {
                int livelloRaggiunto = 0;
                for (int i=tmpCodice.Length-2;i>=0;i--)
                {
                    if (Char.IsNumber(tmpCodice[i]))
                    {
                        int thisNumber = Int32.Parse(tmpCodice[i].ToString());
                        tmpCodice.Remove(i, 1);
                        tmpCodice.Insert(i, ottieniOmocodice(thisNumber));
                        livelloRaggiunto++;
                    }
                    if (livelloRaggiunto == LivelloOmocodia)
                        break;
                }
                // limite sul livello di omocodia?
                if (livelloRaggiunto < LivelloOmocodia)
                    this.LivelloOmocodia = livelloRaggiunto;
                tmpCodice.Remove(15, 1);
                tmpCodice.Append(getCIN(tmpCodice.ToString()));
                this.Codice = tmpCodice.ToString();
            }
            else
            {
                this.Codice = this.CodiceNormalizzato;
            }
        }
        private int ottieniOmocodice(string lettera)
        {
            DataRow[] foundRows = data.Tables["Omocodia"].Select("Lettera = '" + lettera + "'");
            if (foundRows.Length != 1)
                throw new Exception("Errore in data.xml negli omocodici");
            return foundRows[0].Field<int>("Cifra");
        }
        private string ottieniOmocodice(int numero)
        {
            DataRow[] foundRows = data.Tables["Omocodia"].Select("Cifra = " + numero);
            if (foundRows.Length != 1)
                throw new Exception("Errore in data.xml negli omocodici");
            return foundRows[0].Field<string>("Lettera");
        }
        private string calcolaNome(string Nome, bool isCognome = false)
        {
            bool consonanteRimossa = false;
            StringBuilder tmpCodice = new StringBuilder(4);
            foreach (char c in Nome.ToUpper())
            {
                if (consonanti.IndexOf(c) >= 0)
                {
                    tmpCodice.Append(c);
                    if (!consonanteRimossa && !isCognome && tmpCodice.Length == 4)
                    {
                        tmpCodice.Remove(1, 1);
                        consonanteRimossa = true;
                    }
                }
            }
            if (tmpCodice.Length > 3)
                tmpCodice.Remove(3, tmpCodice.Length - 3);
            if (tmpCodice.Length < 3)
            {
                foreach (char c in Nome.ToUpper())
                {
                    if (vocali.IndexOf(c) >= 0)
                        tmpCodice.Append(c);
                    if (tmpCodice.Length == 3)
                        break;
                }
            }
            if (tmpCodice.Length < 3)
            {
                int missingChars = 3 - tmpCodice.Length;
                tmpCodice.Append(new string('X', missingChars));
            }
            return tmpCodice.ToString();
        }
        private void comuneProvinciaDaCodice()
        {
            DataRow[] foundRows = data.Tables["Comuni"].Select("Codice = '" + this.CodiceComune + "'");
            if (foundRows.Length != 1)
                throw new ArgumentException("Codice comune non valido");
            DataRow comuneRow = foundRows[0];
            this.Comune = comuneRow.Field<string>("Nome");
            this.Provincia = comuneRow.Field<string>("Provincia");
        }
        private void codiceDaComuneProvincia()
        {
            string comune = this.Comune.Replace("'", "''");
            DataRow[] foundRows = data.Tables["Comuni"].Select("Nome = '" + comune + "' And Provincia ='" + this.Provincia + "'");
            if (foundRows.Length != 1)
                throw new ArgumentException("Comune e Provincia non trovati");
            DataRow comuneRow = foundRows[0];
            this.CodiceComune = comuneRow.Field<string>("Codice");
        }
        private static int convertiMese(string codice)
        {
            DataRow[] foundRows = data.Tables["Mesi"].Select("Lettera = '" + codice + "'");
            if (foundRows.Length != 1)
                throw new ArgumentException("Codice mese non valido");
            DataRow meseRow = foundRows[0];
            return meseRow.Field<int>("Mese");
        }
        private static string convertiMese(int numero)
        {
            DataRow[] foundRows = data.Tables["Mesi"].Select("Mese = " + numero + "");
            if (foundRows.Length != 1)
                throw new ArgumentException("Mese non valido");
            DataRow meseRow = foundRows[0];
            return meseRow.Field<string>("Lettera");
        }
        private static string getCIN(string codiceParziale)
        {
            int somma = 0;
            for (int i = 0; i < codiceParziale.Length; i++)
            {
                string tableName = "";
                if ((i + 1) % 2 == 1)
                    tableName = "CINDispari";
                else
                    tableName = "CINPari";
                somma += data.Tables[tableName].Select("Carattere = '" + codiceParziale[i] + "'")[0].Field<int>("Valore");
            }
            return data.Tables["CINResto"].Select("Resto = " + (somma%26).ToString())[0].Field<string>("Valore"); ;
        }
        static CodiceFiscale()
        {
            data = new DataSet();
            data.ReadXml(@"data.xml");
            omocodiciRegex = @"[0-9";
            DataTable omocodiaTable = data.Tables["Omocodia"];
            for (int i = 0; i < omocodiaTable.Rows.Count; i++)
                omocodiciRegex += omocodiaTable.Rows[i].Field<string>("Lettera");
            omocodiciRegex += @"]";
            mesiRegex = @"[";
            DataTable mesiTable = data.Tables["Mesi"];
            for (int i = 0; i < mesiTable.Rows.Count; i++)
                mesiRegex += mesiTable.Rows[i].Field<string>("Lettera");
            mesiRegex += @"]";
        }
    }
}
