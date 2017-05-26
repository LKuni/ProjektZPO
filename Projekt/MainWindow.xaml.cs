using Newtonsoft.Json;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Projekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Informer informer_;

        public MainWindow()
        {
            InitializeComponent();
            informer_ = new Informer();

            SQLiteDataAdapter m_oDataAdapter = null;
            DataSet m_oDataSet = null;
            DataTable m_oDataTable = null;
            SQLiteConnection oSQLiteConnection = new SQLiteConnection("Data Source=ZPOProject.s3db");
            SQLiteCommand oCommand = oSQLiteConnection.CreateCommand();
            oCommand.CommandText = "SELECT * FROM UrlCheck";
            m_oDataAdapter = new SQLiteDataAdapter(oCommand.CommandText,
                oSQLiteConnection);
            SQLiteCommandBuilder oCommandBuilder =
                new SQLiteCommandBuilder(m_oDataAdapter);
            m_oDataSet = new DataSet();
            m_oDataAdapter.Fill(m_oDataSet);
            m_oDataTable = m_oDataSet.Tables[0];
            UrlListBox.ItemsSource = m_oDataTable.DefaultView;

            //UrlListBox.ItemsSource = informer_.urls;
        }

        private void SaveUrlButton_Click(object sender, RoutedEventArgs e)
        {
            if (informer_.AddUrl(TitleBox.Text, DescriptionBox.Text))
            {


                SQLiteDataAdapter m_oDataAdapter = null;
                DataSet m_oDataSet = null;
                DataTable m_oDataTable = null;
                SQLiteConnection oSQLiteConnection = new SQLiteConnection("Data Source=ZPOProject.s3db");
                SQLiteCommand oCommand = oSQLiteConnection.CreateCommand();
                oCommand.CommandText = "SELECT * FROM UrlCheck";
                m_oDataAdapter = new SQLiteDataAdapter(oCommand.CommandText,
                    oSQLiteConnection);
                SQLiteCommandBuilder oCommandBuilder =
                    new SQLiteCommandBuilder(m_oDataAdapter);
                m_oDataSet = new DataSet();
                m_oDataAdapter.Fill(m_oDataSet);
                m_oDataTable = m_oDataSet.Tables[0];
                
                DataRow oDataRow = m_oDataTable.NewRow();
                oDataRow[0] = m_oDataTable.Rows.Count + 1;
                oDataRow[2] = TitleBox.Text;
                m_oDataTable.Rows.Add(oDataRow);
                m_oDataAdapter.Update(m_oDataSet);

                UrlListBox.Items.Refresh();
                TitleBox.Text = String.Empty;
                DescriptionBox.Text = String.Empty;

                UrlListBox.ItemsSource = m_oDataTable.DefaultView;

            }
            else
            {
                MessageBox.Show("The URL wasn't added.\nEnter valid URL.", "Oops!");
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            Url selectedUrl = UrlListBox.SelectedItem as Url;
            if (selectedUrl == null)
            {
                MessageBox.Show("Select an url first!");
            }
            else
            {
                String message = "Do you want to delete the following url: \n\n" + informer_.ShowUrlDetails(selectedUrl);
                MessageBoxResult result = MessageBox.Show(message, "Remove?", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    informer_.RemoveUrl(selectedUrl);
                    UrlListBox.Items.Refresh();
                }
            }
        }

        private void UrlListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UrlListBox.SelectedItem != null)
            {
                DataRowView selectedUrl = UrlListBox.SelectedItem as DataRowView;
                string title = selectedUrl.Row["Tytul"] as string;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (UrlListBox.SelectedItem != null)
            {
                DataRowView selectedUrl = UrlListBox.SelectedItem as DataRowView;
                string title = selectedUrl.Row["Tytul"] as string;
                //string fanpageName = GetPageName(title);


                string token = getToken();
                var request = (HttpWebRequest)WebRequest.Create("https://graph.facebook.com/v2.9/"+title+"/feed?access_token=" + token);
                var response = (HttpWebResponse)request.GetResponse();
                FacebookPost fbPost = getFacebookPost(response);
                FacebookPostTextBlock.DataContext = fbPost;

            }
        }

        //private string GetPageName(string title)
        //{
        //    SQLiteDataAdapter m_oDataAdapter = null;
        //    DataSet m_oDataSet = null;
        //    DataTable m_oDataTable = null;
        //    SQLiteConnection oSQLiteConnection = new SQLiteConnection("Data Source=ZPOProject.s3db");
        //    SQLiteCommand oCommand = oSQLiteConnection.CreateCommand();
        //    oCommand.CommandText = "SELECT * FROM UrlCheck WHERE Tytul = '" + title + "'";
        //    m_oDataAdapter = new SQLiteDataAdapter(oCommand.CommandText,
        //        oSQLiteConnection);
        //    SQLiteCommandBuilder oCommandBuilder =
        //        new SQLiteCommandBuilder(m_oDataAdapter);
        //    m_oDataSet = new DataSet();
        //    m_oDataAdapter.Fill(m_oDataSet);
        //    m_oDataTable = m_oDataSet.Tables[0];

        //    DataRow row = m_oDataTable.Rows[0];
        //    string fanpageName = row[1].ToString();

        //    return fanpageName;

        //}

        private string getToken()
        {
            var tokenRequest = (HttpWebRequest)WebRequest.Create("https://graph.facebook.com/v2.9/oauth/access_token?client_id=1597017750339852&client_secret=85ce97dbdce2e302568efdea60aa9f4a&grant_type=client_credentials");

            var tokenResponse = (HttpWebResponse)tokenRequest.GetResponse();

            var responseTokenString = new StreamReader(tokenResponse.GetResponseStream()).ReadToEnd();

            dynamic y = JsonConvert.DeserializeObject(responseTokenString);

            return y.access_token;
        }

        private FacebookPost getFacebookPost(HttpWebResponse response)
        {
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            dynamic x = JsonConvert.DeserializeObject(responseString);
            var data = x.data;
            string message = data[0].message;
            string created_time = data[0].created_time;
            string id = data[0].id;
            FacebookPost fbPost = new FacebookPost(created_time, message, id);
            return fbPost;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            

            DataRowView selectedUrl = UrlListBox.SelectedItem as DataRowView;
            string title = selectedUrl.Row["Tytul"] as string;

            string token = getToken();
            var request = (HttpWebRequest)WebRequest.Create("https://graph.facebook.com/v2.9/" + title + "/feed?access_token=" + token);
            var response = (HttpWebResponse)request.GetResponse();
            FacebookPost fbPost = getFacebookPost(response);

            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";

            // Create an empty page
            PdfPage page = document.AddPage();

            // Get an XGraphics object for drawing
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Create a font
            XFont font = new XFont("Verdana", 14, XFontStyle.Regular);
            XFont headerFont = new XFont("Oblique", 25, XFontStyle.Italic);

            // Draw the text
            gfx.DrawString("Raport", headerFont, XBrushes.Black,
             new XRect(0, 0, page.Width, 100),
             XStringFormats.Center);

            gfx.DrawString(title, headerFont, XBrushes.Black,
             new XRect(0, 0, page.Width, 200),
             XStringFormats.Center);

            gfx.DrawString(fbPost.message, font, XBrushes.Black,
              new XRect(0, 0, page.Width, 520),
              XStringFormats.Center);            

            // Left position in point



            // Save the document...
            const string filename = "Post.pdf";
            document.Save(filename);
            // ...and start a viewer.
            Process.Start(filename);
        
        }
    }
}
