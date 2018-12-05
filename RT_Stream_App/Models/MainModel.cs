using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using RT_Stream_App.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

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

        public static shows.APIData loadShows(companies.companyData selectedCompany, CancellationToken ct)
        {
            shows.APIData toReturn = JsonConvert.DeserializeObject<shows.APIData>(new WebClient().DownloadString("https://svod-be.roosterteeth.com" + selectedCompany.links.shows));
            // If the MainViewModel CancelltationToken requests this to be canceled then it will return null data
            if (ct.IsCancellationRequested)
            {
                return null;
            }
            return toReturn;
        }

        public static shows.APIData loadShowImages(shows.APIData showList, CancellationToken ct)
        {
            shows.APIData toReturn = showList;
            for (int i = 0; i < toReturn.data.Count - 1; i++)
            {
                try
                {
                    toReturn.data[i].thumbImage = downloadedBitmap(toReturn.data[i].included.images[5].attributes.thumb);
                }
                catch (Exception)
                {
                    toReturn.data[i].thumbImage = downloadedBitmap(toReturn.data[i].included.images[3].attributes.thumb);
                }
                if (ct.IsCancellationRequested)
                {
                    return null;
                }
            }
            return toReturn;
        }

        public static IBitmap downloadedBitmap(string DownloadString)
        {
            WebClient wc = new WebClient();
            // Apparently they don't like people accessing the CDN, this is a work around
            wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            byte[] imageBytes = wc.DownloadData(DownloadString);
            MemoryStream ms = new MemoryStream(imageBytes);
            Bitmap toReturn = new Bitmap(ms);
            return toReturn;
        }
    }
}
