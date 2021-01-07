using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CodiceFiscaleUtility;
using System.Data;

namespace CodiceFiscaleGUI
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataSet data;
        private DataView dvComune;
        public MainWindow()
        {
            InitializeComponent();
            data = new DataSet();
            data.ReadXml(@"data.xml");
            DataView province = data.Tables["Comuni"].DefaultView.ToTable(true, "Provincia").DefaultView;
            province.Sort = "Provincia ASC";
            cbProvincia.ItemsSource = province;
            dvComune = new DataView(data.Tables["Comuni"]);
            dvComune.RowFilter = "Provincia = ''";
            dvComune.Sort = "Nome ASC";
            cbComune.ItemsSource = dvComune;
            formUpdated(null,null);
            cfUpdated(null, null);
        }

        private void cbProvincia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dvComune.RowFilter = @"Provincia = '" + cbProvincia.SelectedValue + "'";
        }

        private void formUpdated(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(tbNome.Text) &&
                !String.IsNullOrWhiteSpace(tbCognome.Text) &&
                !String.IsNullOrWhiteSpace((string)cbSesso.SelectedItem) &&
                dpNascita.SelectedDate!=null &&
                !String.IsNullOrWhiteSpace((string)cbComune.SelectedValue) &&
                !String.IsNullOrWhiteSpace((string)cbLivOmocod.SelectedValue))
            {
                CodiceFiscale cf = new CodiceFiscale(tbCognome.Text, tbNome.Text, (string)cbSesso.SelectedItem, (DateTime)dpNascita.SelectedDate, (string)cbComune.SelectedValue, (string)cbProvincia.SelectedValue, Int32.Parse((string)cbLivOmocod.SelectedItem));
                lbCF.Content = cf.Codice;
                Clipboard.SetText(cf.Codice);
            } else
            {
                lbCF.Content = "Codice fiscale non generabile";
            }
        }

        private void changeModalita(object sender, SelectionChangedEventArgs e)
        {
            if( ((ComboBoxItem)cbModalita.SelectedItem).Tag.Equals(@"CFToDati") )
            {
                spDatiToCF.Visibility = Visibility.Collapsed;
                spCFToDati.Visibility = Visibility.Visible;
            } else if(((ComboBoxItem)cbModalita.SelectedItem).Tag.Equals(@"DatiToCF"))
            {
                spCFToDati.Visibility = Visibility.Collapsed;
                spDatiToCF.Visibility = Visibility.Visible;
            }
        }

        private void setInvalidCF()
        {
            lblCognome.Content = @"CF non valido";
            lblNome.Content = @"CF non valido";
            lblSesso.Content = @"CF non valido";
            lblProvincia.Content = @"CF non valido";
            lblComune.Content = @"CF non valido";
            lblNascita.Content = @"CF non valido";
            lblLvlOmocod.Content = @"CF non valido";
        }

        private void cfUpdated(object sender, EventArgs e)
        {
            if (tbCF.Text.Length==16)
            {
                try
                {
                    CodiceFiscale cf = new CodiceFiscale(tbCF.Text);
                    lblCognome.Content = cf.Cognome;
                    lblNome.Content = cf.Nome;
                    lblSesso.Content = cf.Sesso;
                    lblProvincia.Content = cf.Provincia;
                    lblComune.Content = cf.Comune;
                    lblNascita.Content = cf.Nascita.ToShortDateString();
                    lblLvlOmocod.Content = cf.LivelloOmocodia.ToString();
                }
                catch
                {
                    setInvalidCF();
                }
            } else
            {
                setInvalidCF();
            }
        }
    }
}
