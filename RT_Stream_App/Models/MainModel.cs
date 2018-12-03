using Newtonsoft.Json;
using RT_Stream_App.Classes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RT_Stream_App.Models
{
    public static class MainModel
    {
        public static companies.APIData loadCompanies()
        {
            // This takes the API data for companies and converts it into a useable class
            companies.APIData toReturn = JsonConvert.DeserializeObject<companies.APIData>(new WebClient().DownloadString("https://svod-be.roosterteeth.com/api/v1/channels"));
            return toReturn;
        }

        public static shows.APIData loadShows(companies.companyData selectedCompany)
        {
            shows.APIData toReturn = JsonConvert.DeserializeObject<shows.APIData>(new WebClient().DownloadString("https://svod-be.roosterteeth.com" + selectedCompany.links.shows));
            return toReturn;
        }
    }
}
