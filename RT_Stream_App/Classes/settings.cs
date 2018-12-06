using System;
using System.Collections.Generic;
using System.Text;

namespace RT_Stream_App.Classes
{
    public class settings : CallChanged
    {
        private int _page_length;
        private string _username;
        private string _password;

        public int page_length
        {
            get => _page_length;
            set => SetField(ref _page_length, value);
        }
        public string username
        {
            get => _username;
            set => SetField(ref _username, value);
        }
        public string password
        {
            get => _password;
            set => SetField(ref _password, value);
        }
    }
}
