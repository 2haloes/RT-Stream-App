using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace RT_Stream_App.Classes
{

    // API homepage layout
    // [Object array] data
        // [Object] 0,1,2...
            // Show properties


    public class videos : CallChanged
    {
        /// <summary>
        /// Root of the JSON
        /// </summary>
        public class APIData : CallChanged
        {
            private ObservableCollection<videoData> _data;
            private bool _access;

            public APIData()
            {
                access = true;
                this.data = new ObservableCollection<videoData>();
            }
            public ObservableCollection<videoData> data
            {
                get => _data;
                set => SetField(ref _data, value);
            }
            public bool access {
                get => _access;
                set => SetField(ref _access, value);
            }
        }

        /// <summary>
        /// A class that holds the data for each company (Name and link mostly)
        /// </summary>
        public class videoData : CallChanged
        {
            private attributeData _attributes;

            public videoData()
            {
                this.attributes = new attributeData();
            }

            public attributeData attributes {
                get => _attributes;
                set => SetField(ref _attributes, value);
            }
        }

        /// <summary>
        /// Contains the video link
        /// </summary>
        public class attributeData : CallChanged
        {
            private string _url;
            private string _cutUrl;

            public string url {
                get => _url;
                set => SetField(ref _url, value);
            }

            public string cutUrl
            {
                get => _url.Substring(0, _url.LastIndexOf('/'));
            }
        }


    }
}
