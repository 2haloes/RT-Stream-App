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


    public class videos : baseClass
    {
        /// <summary>
        /// Root of the JSON
        /// </summary>
        public new class APIData : baseClass.APIData
        {
            private ObservableCollection<videoData> _data;
            private bool _access;

            public APIData()
            {
                access = true;
                this.data = new ObservableCollection<videoData>();
            }
            public new ObservableCollection<videoData> data
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
        public class videoData : baseClass.objectData
        {
            public videoData()
            {
                attributes = new attributeData();
            }

            private attributeData _attributes;

            public new attributeData attributes
            {
                get => _attributes;
                set => SetField(ref _attributes, value);
            }

        }

        /// <summary>
        /// Contains the video link
        /// </summary>
        public new class attributeData : baseClass.attributeData
        {

            public string url {
                get => displayText;
                set => displayText = value;
            }

            public string cutUrl
            {
                get => displayText.Substring(0, displayText.LastIndexOf('/'));
            }
        }


    }
}
