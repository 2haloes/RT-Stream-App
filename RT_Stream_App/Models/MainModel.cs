using Avalonia.Media.Imaging;
using NETCore.Encrypt;
using NETCore.Encrypt.Internal;
using Newtonsoft.Json;
using RT_Stream_App.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RT_Stream_App.Models
{
    public static class MainModel
    {

        public const string siteURL = "https://svod-be.roosterteeth.com";
        public const string loginURL = "https://auth.roosterteeth.com/oauth/token";
        public static string[] qualityList => new string[] { "240", "360", "480", "720", "1080", "4K" };
        
        /// <summary>
        /// Loads the settings (Or creates the settings file on first load)
        /// </summary>
        /// <returns></returns>
        public static settings SettingsLoad()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "settings.json"))
            {
                return JsonConvert.DeserializeObject<settings>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "settings.json"));
            }
            else
            {
                settings newSettings = new settings()
                {
                    page_length = 20,
                    username = "",
                    password = "",
                    theme = 0,
                    quality = 4
                };
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "settings.json", JsonConvert.SerializeObject(newSettings));
                return newSettings;
            }
        }

        /// <summary>
        /// Loads the themes file or creates the default if one doesn't exist (Custom themes can be created)
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<themes> ThemesLoad()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "themes.json"))
            {
                return JsonConvert.DeserializeObject<ObservableCollection<themes>>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "themes.json"));
            }
            else
            {
                ObservableCollection<themes> toReturn = new ObservableCollection<themes>();
                toReturn.Add(new themes("black", "white", "white", "Light"));
                toReturn.Add(new themes("white", "black", "black", "Dark"));
                toReturn.Add(new themes("black", "white", "crimson", "RT Light"));
                toReturn.Add(new themes("white", "black", "red", "RT Dark"));
                toReturn.Add(new themes("black", "white", "limegreen", "AH Light"));
                toReturn.Add(new themes("white", "black", "limegreen", "AH Dark"));
                toReturn.Add(new themes("black", "white", "orange", "FH Light"));
                toReturn.Add(new themes("white", "black", "orange", "FH Dark"));
                toReturn.Add(new themes("black", "white", "deepskyblue", "SA Light"));
                toReturn.Add(new themes("white", "black", "dodgerblue", "SA Dark"));
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "themes.json", JsonConvert.SerializeObject(toReturn));
                return toReturn;
            }
        }

        public static void aesKeyLoad()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "RT_Stream_App.applicationcfg.json"))
            {
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "RT_Stream_App.applicationcfg.json", EncryptProvider.CreateAesKey().Key);
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

        
        public static void SaveTheme(settings currentSettings)
        {
            settings oldSettings = JsonConvert.DeserializeObject<settings>(File.ReadAllText("settings.json"));
            oldSettings.theme = currentSettings.theme;
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(oldSettings));
        }

        public static void SaveQuality(settings currentSettings)
        {
            settings oldSettings = JsonConvert.DeserializeObject<settings>(File.ReadAllText("settings.json"));
            oldSettings.quality = currentSettings.quality;
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(oldSettings));
        }

        public static void SaveLogin(settings currentSettings)
        {
            settings oldSettings = JsonConvert.DeserializeObject<settings>(File.ReadAllText("settings.json"));
            oldSettings.username = currentSettings.username;
            oldSettings.password = currentSettings.password;
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(oldSettings));
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

                throw;
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

        public static videos.APIData loadVideos(videos.APIData toReturn, HttpClient websiteClient, int selectedQuality, CancellationToken ct)
        {
            // If the MainViewModel CancelltationToken requests this to be canceled then it will return null data
            // If a video is not avliable to view (due to not be open to the public) then the JSON returns an access value
            if (ct.IsCancellationRequested || !toReturn.access)
            {
                return null;
            }
            List<string> fileToOpen;
            HttpResponseMessage response = websiteClient.GetAsync(toReturn.data[0].attributes.url).Result;

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {

                throw;
            }

            fileToOpen = response.Content.ReadAsStringAsync().Result.Split(new string[] { "\n" }, StringSplitOptions.None).ToList();
            fileToOpen = extractQuality(fileToOpen, selectedQuality);
            for (int i = 0; i < fileToOpen.Count; i++)
            {
                // This has somehow been the worst part of the program as of themes being completed, if I have to change this again, I may have to change my approch
                // hls are for newer videos, p.m3u8 is for older videos such as season 1 of Million Dollars But
                if (fileToOpen[i].Contains("hls") || fileToOpen[i].Contains("HLS") || fileToOpen[i].Contains("P.m3u8") || fileToOpen[i].Contains("p.m3u8"))
                {
                    fileToOpen[i] = toReturn.data[0].attributes.cutUrl + "/" + fileToOpen[i];
                }
            }
            if (ct.IsCancellationRequested)
            {
                return null;
            }
            File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + "VideoLink.m3u8", fileToOpen);
            return toReturn;
        }

        public static shows.APIData loadImages(shows.APIData showList, HttpClient websiteClient, CancellationToken ct)
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

        public static episodes.APIData loadImages(episodes.APIData episodeList, HttpClient websiteClient, CancellationToken ct)
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

        public static List<string> extractQuality(List<string> fileContent, int qualityToken)
        {
            //string[] tmpString;
            // NOTE: I actually have no idea if the 4k is correct. It's borderline impossible to find a video with 4K
            if (fileContent.Any(qualityList[qualityToken].Contains) || fileContent.Any("238".Contains))
            {
                for (int i = 0; i < fileContent.Count; i++)
                {
                    if (fileContent[i].Contains("x" + qualityList[qualityToken] + ",") || fileContent[i].Contains("x238,"))
                    {
                        continue;
                    }
                    else if (fileContent[i].Contains("-STREAM-INF"))
                    {
                        fileContent.RemoveRange(i, 2);
                        i--;
                    }
                }
            }
            else if (qualityToken == 0)
            {
                return fileContent;
            }
            else
            {
                return extractQuality(fileContent, (qualityToken - 1));
            }
            return fileContent;
        }

        public static string encryptDetails(string detail)
        {
            if (String.IsNullOrWhiteSpace(detail))
            {
                return "";
            }
            else
            {
                return EncryptProvider.AESEncrypt(detail, File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "RT_Stream_App.applicationcfg.json"));
            }
        }

        public static string decryptDetails(string detail)
        {
            if (String.IsNullOrWhiteSpace(detail))
            {
                return "";
            }
            else
            {
                return EncryptProvider.AESDecrypt(detail, File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "RT_Stream_App.applicationcfg.json"));
            }
        }

        /// <summary>
        /// Logs into the RT Oauth page
        /// After logging in, the information is automatically regestered to the HttpClient
        /// </summary>
        /// <param name="websiteClient"></param>
        /// <param name="usernameCrypt"></param>
        /// <param name="passwordCrypt"></param>
        public static void loginToAPI(HttpClient websiteClient, string usernameCrypt, string passwordCrypt)
        {
            loginPOST toPOST = new loginPOST(decryptDetails(usernameCrypt), decryptDetails(passwordCrypt));
            string playloadString = JsonConvert.SerializeObject(toPOST);
            HttpContent POSTContent = new StringContent(playloadString);
            HttpResponseMessage response = websiteClient.PostAsync(loginURL, POSTContent).Result;

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
