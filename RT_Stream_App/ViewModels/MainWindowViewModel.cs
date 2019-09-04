using Avalonia.Media.Imaging;
using Prism.Commands;
using RT_Stream_App.Classes;
using RT_Stream_App.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

// To anyone reading this, this program pulls from the API and not from web pages like my previous Rooster Teeth program
// The orginal program (based on the old site) downloaded pages as it went to gather the information, this program downloads JSON files from the API instead

namespace RT_Stream_App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            ErrorText = "";
            appSettings = MainModel.SettingsLoad();
            ThemeList = MainModel.ThemesLoad();
            selectedTheme = ThemeList[appSettings.theme];
            QualityList = new ObservableCollection<string>() { "240", "360", "480", "720", "1080", "4K" };
            selectedQuality = QualityList[appSettings.quality];
            Username = MainModel.decryptDetails(appSettings.username);
            Password = MainModel.decryptDetails(appSettings.password);
            // Don't display the setting if there isn't a player included
            UsePlayerDisplay = System.IO.Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "rt-stream-player") && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? true : false;
            // If the option isn't displayed then it should be false
            UsePlayer = UsePlayerDisplay ? appSettings.usePlayer : false;
            websiteClient = new HttpClient();
            // This is used as the CDN rejected requests without a user agent 
            websiteClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            LoadCompanies = new DelegateCommand(async () => await LoadCompaniesAsync());
            ShowLoadText = "Shows";
            ShowsTokenSource = new CancellationTokenSource();
            ShowsToken = ShowsTokenSource.Token;
            LoadShows = new DelegateCommand(async () => await LoadShowsAsync(ShowsToken));
            SeasonTokenSource = new CancellationTokenSource();
            SeasonToken = SeasonTokenSource.Token;
            LoadSeasons = new DelegateCommand(async () => await LoadSeasonsAsync(SeasonToken));
            SeasonLoadText = "";
            EpisodeTokenSource = new CancellationTokenSource();
            EpisodeToken = EpisodeTokenSource.Token;
            LoadEpisodes = new DelegateCommand(async () => await LoadEpisodesAsync(EpisodeToken, 20));
            ButtonEnable = false;
            ButtonText = "Select a video";
            VideoTokenSource = new CancellationTokenSource();
            VideoToken = VideoTokenSource.Token;
            LoadVideo = new DelegateCommand(() => LoadVideoAsync());
            OpenVideo = new DelegateCommand(async () => await LoadVideoPlayerAsync(VideoToken));
            LoginTmp = new DelegateCommand(() => SaveLoginTmp());
            LoginSave = new DelegateCommand(() => SaveLogin());
            LoginAlready = false;
            RecentImage = new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "recent.png");
            LoadCompanies.Execute(null);
        }
        #region Global variables
        private bool _loginAlready;
        private HttpClient _websiteClient;
        private ICommand _loginSave;
        private ICommand _loginTmp;
        private ObservableCollection<string> _qualityList;
        private ObservableCollection<themes> _themeList;
        private settings _appSettings;
        private string _errorText;
        private string _password;
        private string _selectedQuality;
        private string _username;
        private bool _usePlayerDisplay;
        private bool _usePlayer;
        private themes _selectedTheme;

        public Bitmap RefreshIcon => new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "refresh.png");
        public bool LoginAlready { get => _loginAlready; set => SetField(ref _loginAlready, value); }
        // This is passed to all methods that download (for API and video link calls). It is also able to store information which is how the Temp Login feature works
        public HttpClient websiteClient { get => _websiteClient; set => SetField(ref _websiteClient, value); }
        public ICommand LoginSave { get => _loginSave; set => SetField(ref _loginSave, value); }
        public ICommand LoginTmp { get => _loginTmp; set => SetField(ref _loginTmp, value); }
        public ObservableCollection<string> QualityList { get => _qualityList; set => SetField(ref _qualityList, value); }
        public ObservableCollection<themes> ThemeList { get => _themeList; set => SetField(ref _themeList, value); }
        public settings appSettings { get => _appSettings; set => SetField(ref _appSettings, value); }
        public string ErrorText { get => _errorText; set => SetField(ref _errorText, value); }
        public string Password { get => _password; set => SetField(ref _password, value); }
        public string selectedQuality { get => _selectedQuality; set { SetField(ref _selectedQuality, value); appSettings.quality = QualityList.IndexOf(_selectedQuality); MainModel.SaveQuality(appSettings); } }
        public string Username { get => _username; set => SetField(ref _username, value); }
        public bool UsePlayerDisplay { get => _usePlayerDisplay; set => SetField(ref _usePlayerDisplay, value); }
        public bool UsePlayer { get => _usePlayer; set { SetField(ref _usePlayer, value); appSettings.usePlayer = _usePlayer; MainModel.SavePlayerUse(appSettings); } }
        public themes selectedTheme { get => _selectedTheme; set { SetField(ref _selectedTheme, value); appSettings.theme = ThemeList.IndexOf(_selectedTheme); MainModel.SaveTheme(appSettings); } }
        public Avalonia.Controls.WindowIcon ProgramIcon => new Avalonia.Controls.WindowIcon(new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "Rooster.ico"));
        #endregion

        #region Companies variables
        private companies.companyData _selectedCompany;
        private ICommand _loadCompanies;
        private ICommand _loadShows;
        private ObservableCollection<companies.companyData> _companyList;
        
        public companies.companyData selectedCompany { get => _selectedCompany; set { SetField(ref _selectedCompany, value); CancelTokens(1); LoadShows.Execute(null); } }
        public ICommand LoadCompanies { get => _loadCompanies; set => SetField(ref _loadCompanies, value); }
        public ICommand LoadShows { get => _loadShows; set => SetField(ref _loadShows, value); }
        public ObservableCollection<companies.companyData> CompanyList { get => _companyList; set => SetField(ref _companyList, value); }
        #endregion

        #region Shows variables
        private Bitmap _recentImage;
        private CancellationToken _showsToken;
        private CancellationTokenSource _showsTokenSource;
        private ICommand _loadSeasons;
        private ObservableCollection<shows.showData> _showList;
        private shows.showData _selectedShow;
        private string _showLoadText;

        // Unlike the other images, if this is to a getter, it breaks the program (As it is used for to check if the recent episode selection has been made most likely)
        public Bitmap RecentImage { get => _recentImage; set => SetField(ref _recentImage, value); }
        public CancellationToken ShowsToken { get => _showsToken; set => SetField(ref _showsToken, value); }
        public CancellationTokenSource ShowsTokenSource { get => _showsTokenSource; set => SetField(ref _showsTokenSource, value); }
        public ICommand LoadSeasons { get => _loadSeasons; set => SetField(ref _loadSeasons, value); }
        public ObservableCollection<shows.showData> ShowList { get => _showList; set => SetField(ref _showList, value); }
        public shows.showData selectedShow { get => _selectedShow; set { SetField(ref _selectedShow, value); CancelTokens(2); LoadSeasons.Execute(null); } }
        public string ShowLoadText { get => _showLoadText; set => SetField(ref _showLoadText, value); }
        #endregion

        #region Season variables
        private CancellationToken _seasonToken;
        private CancellationTokenSource _seasonTokenSource;
        private ICommand _loadEpisodes;
        private ObservableCollection<seasons.seasonData> _seasonList;
        private seasons.seasonData _selectedSeason;
        private string _seasonLoadText;

        public bool SeasonPlaceholderText { get { return selectedSeason == null ? true : false; } }
        public CancellationToken SeasonToken { get => _seasonToken; set => SetField(ref _seasonToken, value); }
        public CancellationTokenSource SeasonTokenSource { get => _seasonTokenSource; set => SetField(ref _seasonTokenSource, value); }
        public ICommand LoadEpisodes { get => _loadEpisodes; set => SetField(ref _loadEpisodes, value); }
        public ObservableCollection<seasons.seasonData> SeasonList { get => _seasonList; set => SetField(ref _seasonList, value); }
        public seasons.seasonData selectedSeason { get => _selectedSeason; set { SetField(ref _selectedSeason, value); OnPropertyChanged("SeasonPlaceholderText"); CancelTokens(3); LoadEpisodes.Execute(null); } }
        public string SeasonLoadText { get => _seasonLoadText; set => SetField(ref _seasonLoadText, value); }
        #endregion

        #region Episodes variables
        private bool _episodeLoadText;
        private CancellationToken _episodeToken;
        private CancellationTokenSource _episodeTokenSource;
        private episodes.episodeData _selectedEpisode;
        private ICommand _loadVideo;
        private int _pageNumber;
        private ObservableCollection<episodes.episodeData> _episodeList;
        private ObservableCollection<int> _pageList;

        public bool EpisodeLoadText { get => _episodeLoadText; set => SetField(ref _episodeLoadText, value); }
        public bool PagePlaceholderText { get { return PageNumber == 0 ? true : false; } }
        public CancellationToken EpisodeToken { get => _episodeToken; set => SetField(ref _episodeToken, value); }
        public CancellationTokenSource EpisodeTokenSource { get => _episodeTokenSource; set => SetField(ref _episodeTokenSource, value); }
        public episodes.episodeData selectedEpisode { get => _selectedEpisode; set { SetField(ref _selectedEpisode, value); CancelTokens(5); LoadVideo.Execute(null); } }
        public ICommand LoadVideo { get => _loadVideo; set => SetField(ref _loadVideo, value); }
        public int PageCountNumber { get => appSettings.page_length; set { appSettings.page_length = value; MainModel.SavePageCount(appSettings); } }
        public int PageNumber { get => _pageNumber; set { SetField(ref _pageNumber, value); CancelTokens(4); OnPropertyChanged("PagePlaceholderText"); LoadEpisodes.Execute(null); } }
        public ObservableCollection<episodes.episodeData> EpisodeList { get => _episodeList; set => SetField(ref _episodeList, value); }
        public ObservableCollection<int> PageList { get => _pageList; set => SetField(ref _pageList, value); }
        #endregion

        #region Video variables
        private bool _buttonEnable;
        private CancellationToken _videoToken;
        private CancellationTokenSource _videoTokenSource;
        private ICommand _loadVideoPlayer;
        private string _buttonText;

        public bool ButtonEnable { get => _buttonEnable; set => SetField(ref _buttonEnable, value); }
        public CancellationToken VideoToken { get => _videoToken; set => SetField(ref _videoToken, value); }
        public CancellationTokenSource VideoTokenSource { get => _videoTokenSource; set => SetField(ref _videoTokenSource, value); }
        public ICommand OpenVideo { get => _loadVideoPlayer; set => SetField(ref _loadVideoPlayer, value); }
        public string ButtonText { get => _buttonText; set => SetField(ref _buttonText, value); }
        #endregion

        #region Loading from API
        public async Task LoadCompaniesAsync()
        {
            CancelTokens(1);
            try
            {
                CompanyList = await Task.Run(() => MainModel.loadAPI<companies.APIData>("/api/v1/channels", websiteClient, appSettings).data);
            }
            catch (Exception ex)
            {

                ErrorText = "Companies failed to load, please try reloading the program: " + ex.Message;
                return;
            }
            finally
            {
                CompanyList.Insert(0, MainModel.injectListing<companies.companyData>());
            }
        }

        public async Task LoadShowsAsync(CancellationToken ct)
        {
            if (selectedCompany == null)
            {
                return;
            }
            ShowLoadText = "Loading API";
            shows.APIData tmpShows = new shows.APIData();
            try
            {
                tmpShows = await Task.Run(() => MainModel.loadAPI<shows.APIData>(selectedCompany.links.shows, websiteClient, appSettings));
            }
            catch (Exception ex)
            {
                ErrorText = "Shows failed to load, please try again in a minute: " + ex.Message;
                ShowLoadText = "Shows";
                return;
            }
            if (ct.IsCancellationRequested)
            {
                return;
            }
            // After this, thumbnails will display as they load
            ShowList = tmpShows.data;
            ShowList.Insert(0, MainModel.injectListing<shows.showData>(selectedCompany.attributes.slug, RecentImage));
            ShowLoadText = "Loading Thumbnails";
            tmpShows = await Task.Run(() => MainModel.loadImages(tmpShows, websiteClient, ct));
            if (ct.IsCancellationRequested)
            {
                return;
            }
            ShowLoadText = "Shows";
        }

        public async Task LoadSeasonsAsync(CancellationToken ct)
        {
            if (selectedShow == null)
            {
                return;
            }
            if (selectedShow.thumbImage == RecentImage)
            {
                SeasonList = new ObservableCollection<seasons.seasonData>()
                {
                    MainModel.injectListing<seasons.seasonData>(selectedShow.links.seasons)
                };
            }
            else
            {
                SeasonLoadText = "Loading Seasons";
                try
                {
                    SeasonList = await Task.Run(() => MainModel.loadAPI<seasons.APIData>(selectedShow.links.seasons, websiteClient, appSettings).data);
                }
                catch (Exception ex)
                {
                    ErrorText = "Seasons failed to load, please try again in a minute: " + ex.Message;
                    SeasonLoadText = "";
                    return;
                }
            }
            if (ct.IsCancellationRequested)
            {
                return;
            }
            SeasonLoadText = "";
            selectedSeason = SeasonList[0];
        }

        public async Task LoadEpisodesAsync(CancellationToken ct, int pageCount)
        {
            if (selectedSeason == null)
            {
                return;
            }
            SeasonLoadText = "Loading API";
            episodes.APIData tmpEpisodes = new episodes.APIData();
            try
            {
                tmpEpisodes = await Task.Run(() => MainModel.loadAPI<episodes.APIData>(selectedSeason.links.episodes, PageNumber, pageCount, websiteClient));
            }
            catch (Exception ex)
            {
                ErrorText = "Episodes failed to load, please try again in a minute: " + ex.Message;
                SeasonLoadText = "";
                return;
            }
            if (ct.IsCancellationRequested)
            {
                return;
            }
            tmpEpisodes = await Task.Run(() => MainModel.loadEpisodes(tmpEpisodes, ct));
            if (ct.IsCancellationRequested)
            {
                return;
            }
            PageList = new ObservableCollection<int>(Enumerable.Range(1, (tmpEpisodes.total_results / pageCount)));
            // After this, thumbnails will display as they load
            EpisodeList = tmpEpisodes.data;
            SeasonLoadText = "Loading Thumbnails";
            tmpEpisodes = await Task.Run(() => MainModel.loadImages(tmpEpisodes, websiteClient, ct));
            if (ct.IsCancellationRequested)
            {
                return;
            }
            SeasonLoadText = "";
        }

        /// <summary>
        /// Checks if the video can be played
        /// </summary>
        /// <returns></returns>
        public void LoadVideoAsync()
        {
            if (selectedEpisode == null)
            {
                return;
            }
            if ((selectedEpisode.memberTimed || selectedEpisode.sponsorTimed || selectedEpisode.attributes.is_sponsors_only) && (String.IsNullOrWhiteSpace(appSettings.username) || String.IsNullOrWhiteSpace(appSettings.password)) && LoginAlready == false)
            {
                ButtonEnable = false;
                ButtonText = "Please enter login details or select a public video";
                return;
            }
            else
            {
                ButtonEnable = true;
                ButtonText = "Play Video";
                return;
            }
        }

        /// <summary>
        /// Downloads the m3u8 file that contains the stream data. 
        /// VLC automatically uses auto quality
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task LoadVideoPlayerAsync(CancellationToken ct)
        {
            ErrorText = "";
            if (selectedEpisode == null)
            {
                return;
            }
            ButtonText = "Loading API";
            if ((selectedEpisode.memberTimed || selectedEpisode.sponsorTimed || selectedEpisode.attributes.is_sponsors_only) && LoginAlready == false)
            {
                try
                {
                    MainModel.loginToAPI(websiteClient, appSettings.username, appSettings.password);
                    LoginAlready = true;
                }
                catch (Exception ex)
                {

                    ErrorText = "Login failed: " + ex.Message;
                    ButtonText = "Play Video";
                    return;
                }

            }
            videos.APIData tmpVideo = new videos.APIData(); 
            try
            {
                tmpVideo = await Task.Run(() => MainModel.loadAPI<videos.APIData>(selectedEpisode.links.videos, websiteClient, appSettings));
            }
            catch (Exception ex)
            {
                ErrorText = "Video API failed to load, please try again in a minute: " + ex.Message;
            }
            if (ct.IsCancellationRequested)
            {
                return;
            }
            try
            {
                tmpVideo = await Task.Run(() => MainModel.loadVideos(tmpVideo, websiteClient, appSettings.quality, ct));
            }
            catch (Exception ex)
            {
                ErrorText = "Video file failed to load, please try again in a minute: " + ex.Message;
                ButtonText = "Play Video";
                return;
            }
            ButtonText = "Play Video";
            if (ct.IsCancellationRequested)
            {
                return;
            }
            // If set to true, this will use the supplied player, if not, it will use the deafult player
            if (appSettings.usePlayer)
            {
                // Checks if the player is where expected, if not, it reports the error
                if (!System.IO.Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "rt-stream-player"))
                {
                    ErrorText = "RT Player moved or deleted, please either place it back or disable the option";
                    ButtonText = "Play Video";
                    return;
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = AppDomain.CurrentDomain.BaseDirectory + "rt-stream-player/rt-stream-player.exe",
                        Arguments = "\"" + AppDomain.CurrentDomain.BaseDirectory + "VideoLink.m3u8 \"",
                        UseShellExecute = true
                    };
                    await Task.Run(() => Process.Start(psi));
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = AppDomain.CurrentDomain.BaseDirectory + "rt-stream-player/rt-stream-player.AppImage",
                        Arguments = "\"" + AppDomain.CurrentDomain.BaseDirectory + "VideoLink.m3u8\""
                    };
                    // Currently bugged when publishing from Visual Studio, use the dotnet publish command instead
                    await Task.Run(() => Process.Start(psi));
                }
            }
            else
            {
                // This checks the OS as each OS has a different method of opening with the default program
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = AppDomain.CurrentDomain.BaseDirectory + "VideoLink.m3u8",
                        UseShellExecute = true
                    };
                    await Task.Run(() => Process.Start(psi));
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    await Task.Run(() => Process.Start("open", AppDomain.CurrentDomain.BaseDirectory + "VideoLink.m3u8"));
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Currently bugged when publishing from Visual Studio, use the dotnet publish command instead
                    await Task.Run(() => Process.Start("xdg-open", AppDomain.CurrentDomain.BaseDirectory + "VideoLink.m3u8"));
                }
            }
        }
        #endregion

        /// <summary>
        /// This calls the program to cancel the tokens for async tasks and clears the data (1 for company change, 2 for show change, 3 for season change, 4 for page change and 5 for video change)
        /// </summary>
        /// <param name="level"></param>
        public void CancelTokens(int level)
        {
            switch (level)
            {
                case 1:
                    ShowsTokenSource.Cancel();
                    ShowList = null;
                    ShowsTokenSource = new CancellationTokenSource();
                    ShowsToken = ShowsTokenSource.Token;
                    ShowLoadText = "Shows";
                    CancelTokens(2);
                    break;
                case 2:
                    SeasonTokenSource.Cancel();
                    SeasonList = null;
                    SeasonTokenSource = new CancellationTokenSource();
                    SeasonToken = SeasonTokenSource.Token;
                    SeasonLoadText = "";
                    CancelTokens(3);
                    break;
                case 3:
                    EpisodeTokenSource.Cancel();
                    EpisodeList = null;
                    PageList = null;
                    EpisodeTokenSource = new CancellationTokenSource();
                    EpisodeToken = EpisodeTokenSource.Token;
                    ErrorText = "";
                    break;
                case 4:
                    EpisodeTokenSource.Cancel();
                    EpisodeList = null;
                    EpisodeTokenSource = new CancellationTokenSource();
                    EpisodeToken = EpisodeTokenSource.Token;
                    ErrorText = "";
                    break;
                case 5:
                    ButtonEnable = false;
                    ButtonText = "Select a video";
                    VideoTokenSource.Cancel();
                    VideoTokenSource = new CancellationTokenSource();
                    VideoToken = VideoTokenSource.Token;
                    ErrorText = "";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Stores login details for the session and will not be remembered or stored anywhere outside of the current program session
        /// </summary>
        public void SaveLoginTmp()
        {
            appSettings.username = MainModel.encryptDetails(Username);
            appSettings.password = MainModel.encryptDetails(Password);
            LoadVideo.Execute(null);
        }

        /// <summary>
        /// Stores login details on the disk with encryption
        /// If you are extremely worried about security, use the Temp login as no details are stored on the disk
        /// </summary>
        public void SaveLogin()
        {
            SaveLoginTmp();
            MainModel.SaveLogin(appSettings);
        }


        #region PropertyChanged code
        public new event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
