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

        public static settings SettingsLoad()
        {
            if (File.Exists("settings.json"))
            {
                return JsonConvert.DeserializeObject<settings>(File.ReadAllText("settings.json"));
            }
            else
            {
                settings newSettings = new settings()
                {
                    page_length = 20,
                    username = "",
                    password = "",
                    theme = "Light"
                };
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(newSettings));
                return newSettings;
            }
        }

        public static void SavePageCount(settings currentSettings)
        {
            if (currentSettings.page_length != 0)
            {
                settings oldSettings = JsonConvert.DeserializeObject<settings>(File.ReadAllText("settings.json"));
                oldSettings.page_length = currentSettings.page_length;
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(oldSettings));
            }
        }

        public static TOut loadJSON<TOut>(string nextLink)
        {
            TOut toReturn = JsonConvert.DeserializeObject<TOut>(new WebClient().DownloadString("https://svod-be.roosterteeth.com" + nextLink));
            return toReturn;
        }

        public static TOut loadJSON<TOut>(string nextLink, int pageCount, int perPage)
        {
            TOut toReturn = JsonConvert.DeserializeObject<TOut>(new WebClient().DownloadString("https://svod-be.roosterteeth.com" + nextLink + "?page=" + pageCount + "&per_page=" + perPage));
            return toReturn;
        }

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

        public static seasons.APIData loadSeasons(shows.showData selectedShow, CancellationToken ct)
        {
            seasons.APIData toReturn = JsonConvert.DeserializeObject<seasons.APIData>(new WebClient().DownloadString("https://svod-be.roosterteeth.com" + selectedShow.links.seasons));
            // If the MainViewModel CancelltationToken requests this to be canceled then it will return null data
            if (ct.IsCancellationRequested)
            {
                return null;
            }
            return toReturn;
        }

        public static episodes.APIData loadEpisodes(episodes.APIData toReturn, CancellationToken ct)
        {
            //episodes.APIData toReturn = JsonConvert.DeserializeObject<episodes.APIData>(new WebClient().DownloadString("https://svod-be.roosterteeth.com" + selectedSeason.links.episodes + "?page=" + pageCount + "&per_page=" + perPage));
            for (int i = 0; i < toReturn.data.Count; i++)
            {
                // If the MainViewModel CancelltationToken requests this to be canceled then it will return null data
                if (ct.IsCancellationRequested)
                {
                    return null;
                }
                if (!toReturn.data[i].attributes.is_sponsors_only)
                {
                    if (toReturn.data[i].attributes.public_golive_at > DateTime.Now)
                    {
                        if (toReturn.data[i].attributes.member_golive_at < DateTime.Now)
                        {
                            toReturn.data[i].memberTimed = true;
                        }
                        else
                        {
                            toReturn.data[i].sponsorTimed = true;
                        }
                    }
                }
            }
            return toReturn;
        }

        public static videos.APIData loadVideos(episodes.episodeData selectedEpisode, CancellationToken ct)
        {
            videos.APIData toReturn = JsonConvert.DeserializeObject<videos.APIData>(new WebClient().DownloadString("https://svod-be.roosterteeth.com" + selectedEpisode.links.videos));
            // If the MainViewModel CancelltationToken requests this to be canceled then it will return null data
            // If a video is not avliable to view (due to not be open to the public) then the JSON returns an access value
            if (ct.IsCancellationRequested || !toReturn.access)
            {
                return null;
            }
            string[] fileToOpen;
            using (WebClient webClient = new WebClient())
                fileToOpen = webClient.DownloadString(toReturn.data[0].attributes.url).Split(new string[] { "\n" }, StringSplitOptions.None);
            for (int i = 0; i < fileToOpen.Length; i++)
            {
                if (fileToOpen[i].Contains("-store-"))
                {
                    fileToOpen[i] = toReturn.data[0].attributes.cutUrl + "/" + fileToOpen[i];
                }
            }
            if (ct.IsCancellationRequested)
            {
                return null;
            }
            File.WriteAllLines("VideoLink.m3u8", fileToOpen);
            return toReturn;
        }



        public static shows.APIData loadShowImages(shows.APIData showList, CancellationToken ct)
        {
            shows.APIData toReturn = showList;
            for (int i = 0; i < toReturn.data.Count; i++)
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

        public static episodes.APIData loadEpisodeImages(episodes.APIData episodeList, CancellationToken ct)
        {
            episodes.APIData toReturn = episodeList;
            for (int i = 0; i < toReturn.data.Count; i++)
            {
                toReturn.data[i].Image = downloadedBitmap(toReturn.data[i].included.images[0].attributes.medium);
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
