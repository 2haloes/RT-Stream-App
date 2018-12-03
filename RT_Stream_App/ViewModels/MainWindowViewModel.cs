﻿using RT_Stream_App.Classes;
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

// To anyone reading this, this program pulls from the API and not from web pages like my previous Rooster Teeth program
// The orginal program (based on the old site) downloaded pages as it went to gather the information, this program downloads JSON files from the API instead

namespace RT_Stream_App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        // use => instead of = for assigning
        // public string Greeting => "I am testing!";

        #region Companies variables
        private companies.companyData _selectedCompany;
        public companies.companyData selectedCompany { get => _selectedCompany; set { SetField(ref _selectedCompany, value); ShowList = MainModel.loadShows(selectedCompany).data; } }
        public ObservableCollection<companies.companyData> CompanyList => MainModel.loadCompanies().data;
        #endregion

        #region Shows variables
        private shows.showData _selectedShow;
        private ObservableCollection<shows.showData> _showList;

        public shows.showData selectedShow { get => _selectedShow; set { SetField(ref _selectedShow, value); } }
        public ObservableCollection<shows.showData> ShowList { get => _showList; set => SetField(ref _showList, value); }
        #endregion



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
