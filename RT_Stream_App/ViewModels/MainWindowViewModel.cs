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
            ShowsLoadingBool = false;
            LoadShows = new DelegateCommand(async () => await LoadShowsAsync());
        }

        #region Companies variables
        private companies.companyData _selectedCompany;
        public companies.companyData selectedCompany { get => _selectedCompany; set { SetField(ref _selectedCompany, value); LoadShows.Execute(null); } }
        public ObservableCollection<companies.companyData> CompanyList => MainModel.loadCompanies().data;

        public ICommand LoadShows;
        #endregion

        #region Shows variables
        private shows.showData _selectedShow;
        private ObservableCollection<shows.showData> _showList;
        private string _showLoadText;
        private bool _showsLoadingBool;

        public bool ShowsLoadingBool { get => _showsLoadingBool; set => SetField( ref _showsLoadingBool, value); }
        public string ShowLoadText { get => _showLoadText; set => SetField(ref _showLoadText, value); }
        public shows.showData selectedShow { get => _selectedShow; set { SetField(ref _selectedShow, value); } }
        public ObservableCollection<shows.showData> ShowList { get => _showList; set => SetField(ref _showList, value); }
        #endregion

        public async Task LoadShowsAsync()
        {
            ShowsLoadingBool = true;
            ShowLoadText = "Loading API";
            shows.APIData tmpShows = await Task.Run(() => MainModel.loadShows(selectedCompany));
            ShowLoadText = "Loading Thumbnails";
            tmpShows = await Task.Run(() => MainModel.loadShowImages(tmpShows));
            ShowsLoadingBool = false;
            ShowLoadText = "Shows";
            ShowList = tmpShows.data;
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
