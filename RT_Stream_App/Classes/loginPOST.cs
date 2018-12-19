namespace RT_Stream_App.Classes
{
    /// <summary>
    /// This is the data sent to login to the website
    /// This is used in the mainmodel class file
    /// </summary>
    public class loginPOST: CallChanged
    {

        public loginPOST(string user, string pass)
        {
            client_id = "4338d2b4bdc8db1239360f28e72f0d9ddb1fd01e7a38fbb07b4b1f4ba4564cc5";
            grant_type = "password";
            username = user;
            password = pass;
            scope = "user public";
        }

        private string _client_id;
        private string _grant_type;
        private string _username;
        private string _password;
        private string _scope;

        public string client_id { get => _client_id; set => SetField(ref _client_id, value); }
        public string grant_type { get => _grant_type; set => SetField(ref _grant_type, value); }
        public string username { get => _username; set => SetField(ref _username, value); }
        public string password { get => _password; set => SetField(ref _password, value); }
        public string scope { get => _scope; set => SetField(ref _scope, value); }
    }
}
