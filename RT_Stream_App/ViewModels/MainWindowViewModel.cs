using Newtonsoft.Json;
using RT_Stream_App.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

// To anyone reading this, this program pulls from the API and not from web pages like my previous Rooster Teeth program
// The orginal program (based on the old site) downloaded pages as it went to gather the information, this program downloads JSON files from the API instead

namespace RT_Stream_App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // use => instead of = for assigning
        // public string Greeting => "I am testing!";

        public companies.APIData siteList => loadCompanies();

        //public string Greeting => TestLoop(siteList);
        public ObservableCollection<companies.companyData> CompanyList => siteList.data;

        public companies.APIData loadCompanies()
        {
            // This takes the API data for companies and converts it into a useable class
            companies.APIData toReturn = JsonConvert.DeserializeObject<companies.APIData>(new WebClient().DownloadString("https://svod-be.roosterteeth.com/api/v1/channels"));
            return toReturn;
        }
    }
}
