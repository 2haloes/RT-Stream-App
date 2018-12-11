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


    public class baseClass : CallChanged
    {
        /// <summary>
        /// Root of the JSON
        /// </summary>
        public class APIData : CallChanged
        {
            private ObservableCollection<objectData> _data;

            public APIData()
            {
                this.data = new ObservableCollection<objectData>();
            }
            public ObservableCollection<objectData> data {
                get => _data;
                set => SetField(ref _data, value);
            }
        }

        /// <summary>
        /// A class that holds the data for each company (Name and link mostly)
        /// </summary>
        public class objectData : CallChanged
        {
            private linkData _links;
            private attributeData _attributes;

            public objectData()
            {
                this.attributes = new attributeData();
                this.links = new linkData();
            }

            public attributeData attributes {
                get => _attributes;
                set => SetField(ref _attributes, value);
            }
            public linkData links
            {
                get => _links;
                set => SetField(ref _links, value);
            }
        }

        /// <summary>
        /// Contains the company name
        /// </summary>
        public class attributeData : CallChanged
        {
            private string _displayText;

            public string displayText {
                get => _displayText;
                set => SetField(ref _displayText, value);
            }
        }

        /// <summary>
        /// Contains link data for the next step
        /// </summary>
        public class linkData : CallChanged
        {
            private string _nextLink;

            public string nextLink {
                get => _nextLink;
                set => SetField(ref _nextLink, value);
            }
        }


    }
}
