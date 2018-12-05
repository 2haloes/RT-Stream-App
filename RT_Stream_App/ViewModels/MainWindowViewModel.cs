using Prism.Commands;
using RT_Stream_App.Classes;
using RT_Stream_App.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

// To anyone reading this, this program pulls from the API and not from web pages like my previous Rooster Teeth program
// The orginal program (based on the old site) downloaded pages as it went to gather the information, this program downloads JSON files from the API instead

namespace RT_Stream_App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        // use => instead of = for assigning
        // public string Greeting => "I am testing!";

        public MainWindowViewModel()
        {
            ShowLoadText = "Shows";
            ShowsTokenSource = new CancellationTokenSource();
            ShowsToken = ShowsTokenSource.Token;
            LoadShows = new DelegateCommand(async () => await LoadShowsAsync(ShowsToken));
            SeasonTokenSource = new CancellationTokenSource();
            SeasonToken = SeasonTokenSource.Token;
            LoadSeasons = new DelegateCommand(async () => await LoadSeasonsAsync(ShowsToken));
            SeasonLoadText = false;
        }
        #region Global variables

        #endregion

        #region Companies variables
        private companies.companyData _selectedCompany;
        public companies.companyData selectedCompany { get => _selectedCompany; set { SetField(ref _selectedCompany, value); CancelTokens(1); LoadShows.Execute(null); } }
        public ObservableCollection<companies.companyData> CompanyList => MainModel.loadCompanies().data;

        public ICommand LoadShows;
        #endregion

        #region Shows variables
        private shows.showData _selectedShow;
        private ObservableCollection<shows.showData> _showList;
        private string _showLoadText;
        private CancellationTokenSource _showsTokenSource;
        private CancellationToken _showsToken;

        public string ShowLoadText { get => _showLoadText; set => SetField(ref _showLoadText, value); }
        public shows.showData selectedShow { get => _selectedShow; set { SetField(ref _selectedShow, value); LoadSeasons.Execute(null); } }
        public ObservableCollection<shows.showData> ShowList { get => _showList; set => SetField(ref _showList, value); }
        public CancellationTokenSource ShowsTokenSource { get => _showsTokenSource; set => SetField(ref _showsTokenSource, value); }
        public CancellationToken ShowsToken { get => _showsToken; set => SetField(ref _showsToken, value); }

        public ICommand LoadSeasons;
        #endregion

        #region Season variables
        private CancellationTokenSource _seasonTokenSource;
        private CancellationToken _seasonToken;
        private ObservableCollection<seasons.seasonData> _seasonList;
        private seasons.seasonData _selectedSeason;
        private bool _seasonLoadText;

        public CancellationTokenSource SeasonTokenSource { get => _seasonTokenSource; set => SetField(ref _seasonTokenSource, value); }
        public CancellationToken SeasonToken { get => _seasonToken; set => SetField(ref _seasonToken, value); }
        public ObservableCollection<seasons.seasonData> SeasonList { get => _seasonList; set => SetField(ref _seasonList, value); }
        public bool SeasonLoadText { get => _seasonLoadText; set => SetField(ref _seasonLoadText, value); }
        public seasons.seasonData selectedSeason { get => _selectedSeason; set { SetField(ref _selectedSeason, value); } }
        #endregion

        #region Episodes variables
        private CancellationTokenSource _episodeTokenSource;
        private CancellationToken _episodeToken;
        private ObservableCollection<episodes.episodeData> _episodeList;
        private episodes.episodeData _selectedEpisode;
        private bool _episodeLoadText;

        public CancellationTokenSource EpisodeTokenSource { get => _episodeTokenSource; set => SetField(ref _episodeTokenSource, value); }
        public CancellationToken EpisodeToken { get => _episodeToken; set => SetField(ref _episodeToken, value); }
        public ObservableCollection<episodes.episodeData> EpisodeList { get => _episodeList; set => SetField(ref _episodeList, value); }
        public bool EpisodeLoadText { get => _episodeLoadText; set => SetField(ref _episodeLoadText, value); }
        public episodes.episodeData selectedEpisode { get => _selectedEpisode; set { SetField(ref _selectedEpisode, value); } }
        #endregion

        public async Task LoadShowsAsync(CancellationToken ct)
        {
            ShowLoadText = "Loading API";
            shows.APIData tmpShows = await Task.Run(() => MainModel.loadShows(selectedCompany, ct));
            if (ct.IsCancellationRequested)
            {
                return;
            }
            // After this, thumbnails will display as they load
            ShowList = tmpShows.data;
            ShowLoadText = "Loading Thumbnails";
            tmpShows = await Task.Run(() => MainModel.loadShowImages(tmpShows, ct));
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
            SeasonLoadText = true;
            SeasonList = await Task.Run(() => MainModel.loadSeasons(selectedShow, ct).data);
            if (ct.IsCancellationRequested)
            {
                return;
            }
            SeasonLoadText = false;
        }

        // TODO: Episode loading, showing vars based on public/member/sponsor only (From datetimes on API) and page loading

            public void CancelTokens(int level)
        {
            switch (level)
            {
                case 1:
                    ShowsTokenSource.Cancel();
                    ShowList = null;
                    ShowsTokenSource = new CancellationTokenSource();
                    ShowsToken = ShowsTokenSource.Token;
                    CancelTokens(2);
                    break;
                case 2:
                    SeasonTokenSource.Cancel();
                    SeasonList = null;
                    SeasonTokenSource = new CancellationTokenSource();
                    SeasonToken = SeasonTokenSource.Token;
                    break;
                default:
                    break;
            }
        }

        #region PropertyChanged code
        public event PropertyChangedEventHandler PropertyChanged;
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
