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
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace RT_Stream_App.Models
{
    public static class MainModel
    {

        public const string siteURL = "https://svod-be.roosterteeth.com";
        public const string loginURL = "https://auth.roosterteeth.com/oauth/token";
        public static readonly string settingFile = (AppDomain.CurrentDomain.BaseDirectory + "settings.json");
        public static string[] qualityList => new string[] { "240", "360", "480", "720", "1080", "4K" };

        /// <summary>
        /// Loads the settings (Or creates the settings file on first load)
        /// </summary>
        /// <returns></returns>
        public static settings SettingsLoad()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "RT_Stream_App.applicationcfg.json"))
            {
                // Only for next release to transition from the old method to the new one
                if (File.Exists(settingFile))
                {
                    settings newSettings = JsonConvert.DeserializeObject<settings>(File.ReadAllText(settingFile));
                    if (!String.IsNullOrWhiteSpace(newSettings.username) || !String.IsNullOrWhiteSpace(newSettings.password))
                    {
                        string username = EncryptProvider.AESDecrypt(newSettings.username, File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "RT_Stream_App.applicationcfg.json"));
                        string password = EncryptProvider.AESDecrypt(newSettings.password, File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "RT_Stream_App.applicationcfg.json"));
                        newSettings.username = encryptDetails(newSettings.username);
                        newSettings.password = encryptDetails(newSettings.password);
                        File.WriteAllText(settingFile, JsonConvert.SerializeObject(newSettings));
                    }
                }
                
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "RT_Stream_App.applicationcfg.json");
                
            }

            if (File.Exists(settingFile))
            {
                return JsonConvert.DeserializeObject<settings>(File.ReadAllText(settingFile));
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
                File.WriteAllText(settingFile, JsonConvert.SerializeObject(newSettings));
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

        /// <summary>
        /// Saves the amount of episodes to load per page when the value is changed
        /// </summary>
        /// <param name="currentSettings"></param>
        public static void SavePageCount(settings currentSettings)
        {
            if (currentSettings.page_length != 0)
            {
                settings oldSettings = JsonConvert.DeserializeObject<settings>(File.ReadAllText(settingFile));
                oldSettings.page_length = currentSettings.page_length;
                File.WriteAllText(settingFile, JsonConvert.SerializeObject(oldSettings));
            }
        }


        public static void SaveTheme(settings currentSettings)
        {
            settings oldSettings = JsonConvert.DeserializeObject<settings>(File.ReadAllText(settingFile));
            oldSettings.theme = currentSettings.theme;
            File.WriteAllText(settingFile, JsonConvert.SerializeObject(oldSettings));
        }

        public static void SaveQuality(settings currentSettings)
        {
            settings oldSettings = JsonConvert.DeserializeObject<settings>(File.ReadAllText(settingFile));
            oldSettings.quality = currentSettings.quality;
            File.WriteAllText(settingFile, JsonConvert.SerializeObject(oldSettings));
        }

        public static void SaveLogin(settings currentSettings)
        {
            settings oldSettings = JsonConvert.DeserializeObject<settings>(File.ReadAllText(settingFile));
            oldSettings.username = currentSettings.username;
            oldSettings.password = currentSettings.password;
            File.WriteAllText(settingFile, JsonConvert.SerializeObject(oldSettings));
        }

        public static void SavePlayerUse(settings currentSettings)
        {
            settings oldSettings = JsonConvert.DeserializeObject<settings>(File.ReadAllText(settingFile));
            oldSettings.usePlayer = currentSettings.usePlayer;
            File.WriteAllText(settingFile, JsonConvert.SerializeObject(oldSettings));
        }

        public static TOut loadAPI<TOut>(string refLink, HttpClient websiteClient, settings settingsVars)
        {
            HttpResponseMessage response = websiteClient.GetAsync(siteURL + refLink).Result;

            if (response.StatusCode == HttpStatusCode.Forbidden && !(String.IsNullOrWhiteSpace(settingsVars.username) || String.IsNullOrWhiteSpace(settingsVars.password)))
            {
                loginToAPI(websiteClient, settingsVars.username, settingsVars.password);
                response = websiteClient.GetAsync(siteURL + refLink).Result;
            }

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
                throw;
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
                string tmpChar = toReturn.data[i].attributes.channel_slug[0].ToString();
                string tmpShowName = toReturn.data[i].attributes.show_slug;
                toReturn.data[i].seriesDisplay = toReturn.data[i].attributes.channel_slug;
                toReturn.data[i].seriesDisplay = toReturn.data[i].seriesDisplay.Remove(0, 1).Insert(0, tmpChar.ToUpper());
                tmpChar = tmpShowName[0].ToString();
                tmpShowName = tmpShowName.Remove(0, 1).Insert(0, tmpChar.ToUpper());
                for (int channel_i = 0; channel_i < toReturn.data[i].seriesDisplay.Length; channel_i++)
                {
                    if (toReturn.data[i].seriesDisplay[channel_i] == '-')
                    {
                        tmpChar = toReturn.data[i].seriesDisplay[channel_i + 1].ToString();
                        toReturn.data[i].seriesDisplay = toReturn.data[i].seriesDisplay.Remove(channel_i, 2).Insert(channel_i, " " + tmpChar.ToUpper());
                    }
                }

                for (int show_i = 0; show_i < tmpShowName.Length; show_i++)
                {
                    if (tmpShowName[show_i] == '-' && tmpShowName[show_i - 1] == 't' && tmpShowName[show_i + 1] == 's')
                    {
                        tmpShowName = tmpShowName.Remove(show_i, 1).Insert(show_i, "'");
                    }
                    else if (tmpShowName[show_i] == '-')
                    {
                        tmpChar = tmpShowName[show_i + 1].ToString();
                        tmpShowName = tmpShowName.Remove(show_i, 2).Insert(show_i, " " + tmpChar.ToUpper());
                    }
                }

                toReturn.data[i].seriesDisplay += " - " + tmpShowName;
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
            if (fileToOpen.Count < 3)
            {
                // This splits at # but keeps the char instead of removing it
                fileToOpen = Regex.Split(response.Content.ReadAsStringAsync().Result, @"?<=[#]").ToList();
            }
            fileToOpen = extractQuality(fileToOpen, selectedQuality);
            for (int i = 0; i < fileToOpen.Count; i++)
            {
                // This has somehow been the worst part of the program as of themes being completed, if I have to change this again, I may have to change my approch
                // One video changed how videos are laid out so now I'm adding more to this as it's different
                // hls are for newer videos, p.m3u8 is for older videos such as season 1 of Million Dollars But
                if (fileToOpen[i].Contains("hls") || fileToOpen[i].Contains("HLS") || fileToOpen[i].Contains("P.m3u8") || fileToOpen[i].Contains("p.m3u8"))
                {
                    // This is done like this because the playlist file URL is located on it's own line
                    fileToOpen[i] = toReturn.data[0].attributes.cutUrl + "/" + fileToOpen[i];
                }

                // This should cover audio tracks and I-frames (If anyone cares about I-frames in this case)
                if (fileToOpen[i].Contains("URI="))
                {
                    int addIndex = fileToOpen[i].IndexOf("URI=") + 5;
                    // This is done like this because the audio/I-frame files are located in the middle of strings, thankfully a lot less indexes than the last version of this program
                    fileToOpen[i] = fileToOpen[i].Insert(addIndex, toReturn.data[0].attributes.cutUrl + "/");
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
            for (int i = 1; i < toReturn.data.Count; i++)
            {
                var logoImage = toReturn.data[i].included.images.FirstOrDefault(img => img.attributes.image_type == "profile");
                if (logoImage != null)
                {
                    toReturn.data[i].thumbImage = downloadedBitmap(logoImage.attributes.thumb, websiteClient);
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
                var logoImage = toReturn.data[i].included.images.FirstOrDefault(img => img.attributes.image_type == "profile");
                if (logoImage != null)
                {
                    toReturn.data[i].Image = downloadedBitmap(logoImage.attributes.medium, websiteClient);
                }
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

        public static TOut injectListing<TOut>([Optional, DefaultParameterValue("")]string linkData, [Optional, DefaultParameterValue(null)]Bitmap recentImage)
        {
            if (typeof(TOut) == typeof(companies.companyData))
            {
                return (TOut)(object)new companies.companyData
                {
                    attributes = new companies.attributeData
                    {
                        name = "All",
                        slug = "all"
                    },
                    links = new companies.linkData
                    {
                        shows = "/api/v1/shows/"
                    }
                };
            }
            else if (typeof(TOut) == typeof(shows.showData))
            {
                return (TOut)(object)new shows.showData()
                {
                    attributes = new shows.attributeData()
                    {
                        title = "Recent episodes",
                        is_sponsors_only = false
                    },
                    links = new shows.linkData()
                    {
                        seasons = linkData == "all" ? "/api/v1/episodes?page=1" : "/api/v1/channels/" + linkData + "/episodes?page=1"
                    },
                    thumbImage = recentImage
                };
            }
            else if (typeof(TOut) == typeof(seasons.seasonData))
            {
                return (TOut)(object)new seasons.seasonData()
                {
                    attributes = new seasons.attributeData()
                    {
                        title = "Recent Episodes"
                    },
                    links = new seasons.linkData()
                    {
                        episodes = linkData
                    }
                };
            }
            else
            {
                return default(TOut);
            }
        }

        public static List<string> extractQuality(List<string> fileContent, int qualityToken)
        {
            List<string> compList = new List<string>();
            for (int i = 0; i < fileContent.Count; i++)
            {
                if (!String.IsNullOrWhiteSpace(fileContent[i]))
                {
                    if (fileContent[i].Substring(0, 1) == "#")
                    {
                        compList.Add(fileContent[i]);
                    }
                }

            }
            // NOTE: I actually have no idea if the 4k is correct. It's borderline impossible to find a video with 4K
            if (compList.Any(item => item.Contains(qualityList[qualityToken])))
            {
                for (int i = 0; i < fileContent.Count; i++)
                {
                    if (fileContent[i].Contains("x" + qualityList[qualityToken] + ","))
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
            else if (qualityToken == 0 && compList.Any(item => item.Contains("238")))
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
            else if (qualityToken == 0 && !compList.Any(item => item.Contains("238")))
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
                return EncryptProvider.AESEncrypt(detail, getAESKey());
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
                return EncryptProvider.AESDecrypt(detail, getAESKey());
            }
        }

        /// <summary>
        /// Generates a key based on the computer name and user without storing the key on disk
        /// </summary>
        /// <returns></returns>
        public static string getAESKey()
        {
            string keyString = "";
            keyString += Environment.GetEnvironmentVariable("COMPUTERNAME") ?? Environment.GetEnvironmentVariable("HOSTNAME");
            keyString += Environment.UserName;
            return EncryptProvider.Sha256(keyString).Substring(0,32);
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
