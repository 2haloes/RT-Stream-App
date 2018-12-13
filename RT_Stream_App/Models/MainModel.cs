using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using RT_Stream_App.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RT_Stream_App.Models
{
    public static class MainModel
    {

        public const string siteURL = "https://svod-be.roosterteeth.com";

        /// <summary>
        /// Loads the settings (Or creates the settings file on first load)
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Saves the amount of episodes to load per page when the value is changed
        /// </summary>
        /// <param name="currentSettings"></param>
        public static void SavePageCount(settings currentSettings)
        {
            if (currentSettings.page_length != 0)
            {
                settings oldSettings = JsonConvert.DeserializeObject<settings>(File.ReadAllText("settings.json"));
                oldSettings.page_length = currentSettings.page_length;
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(oldSettings));
            }
        }

        public static TOut loadAPI<TOut>(string refLink, HttpClient websiteClient)
        {
            HttpResponseMessage response = websiteClient.GetAsync(siteURL + refLink).Result;

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {

                return default(TOut);
            }

            TOut toReturn = JsonConvert.DeserializeObject<TOut>(response.Content.ReadAsStringAsync().Result);
            return toReturn;
        }

        public static TOut loadAPI<TOut>(string refLink, int pageCount, int perPage, HttpClient websiteClient)
        {
            HttpResponseMessage response = websiteClient.GetAsync(siteURL + refLink + "?page=" + pageCount + "&per_page=" + perPage).Result;

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {

                return default(TOut);
            }

            TOut toReturn = JsonConvert.DeserializeObject<TOut>(response.Content.ReadAsStringAsync().Result);
            return toReturn;
        }

        public static episodes.APIData loadEpisodes(episodes.APIData toReturn, CancellationToken ct)
        {
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

        public static videos.APIData loadVideos(videos.APIData toReturn, HttpClient websiteClient, CancellationToken ct)
        {
            // If the MainViewModel CancelltationToken requests this to be canceled then it will return null data
            // If a video is not avliable to view (due to not be open to the public) then the JSON returns an access value
            if (ct.IsCancellationRequested || !toReturn.access)
            {
                return null;
            }
            string[] fileToOpen;
            HttpResponseMessage response = websiteClient.GetAsync(toReturn.data[0].attributes.url).Result;

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {

                throw;
            }

            fileToOpen = response.Content.ReadAsStringAsync().Result.Split(new string[] { "\n" }, StringSplitOptions.None);
            for (int i = 0; i < fileToOpen.Length; i++)
            {
                if (fileToOpen[i].Contains("hls") || fileToOpen[i].Contains("HLS"))
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



        public static shows.APIData loadShowImages(shows.APIData showList, HttpClient websiteClient, CancellationToken ct)
        {
            shows.APIData toReturn = showList;
            for (int i = 0; i < toReturn.data.Count; i++)
            {
                try
                {
                    toReturn.data[i].thumbImage = downloadedBitmap(toReturn.data[i].included.images[5].attributes.thumb, websiteClient);
                }
                catch (Exception)
                {
                    toReturn.data[i].thumbImage = downloadedBitmap(toReturn.data[i].included.images[3].attributes.thumb, websiteClient);
                }
                if (ct.IsCancellationRequested)
                {
                    return null;
                }
            }
            return toReturn;
        }

        public static episodes.APIData loadEpisodeImages(episodes.APIData episodeList, HttpClient websiteClient, CancellationToken ct)
        {
            episodes.APIData toReturn = episodeList;
            for (int i = 0; i < toReturn.data.Count; i++)
            {
                toReturn.data[i].Image = downloadedBitmap(toReturn.data[i].included.images[0].attributes.medium, websiteClient);
                if (ct.IsCancellationRequested)
                {
                    return null;
                }
            }
            return toReturn;
        }

        public static IBitmap downloadedBitmap(string DownloadString, HttpClient websiteClient)
        {
            HttpResponseMessage response = websiteClient.GetAsync(DownloadString).Result;

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {

                throw;
            }

            Bitmap toReturn = new Bitmap(response.Content.ReadAsStreamAsync().Result);
            return toReturn;
        }
    }
}
