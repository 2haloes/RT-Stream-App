namespace RT_Stream_App.Classes
{
    public class settings : CallChanged
    {
        private int _page_length;
        private string _username;
        private string _password;
        private int _theme;
        private int _quality;

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
        public int theme
        {
            get => _theme;
            set => SetField(ref _theme, value);
        }

        public int quality
        {
            get => _quality;
            set => SetField(ref _quality, value);
        }
    }
}
