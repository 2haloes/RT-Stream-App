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


    public class seasons : CallChanged
    {
        /// <summary>
        /// Root of the JSON
        /// </summary>
        public class APIData : CallChanged
        {
            private ObservableCollection<seasonData> _data;

            public APIData()
            {
                this.data = new ObservableCollection<seasonData>();
            }
            public ObservableCollection<seasonData> data {
                get => _data;
                set => SetField(ref _data, value);
            }
        }

        /// <summary>
        /// A class that holds the data for each company (Name and link mostly)
        /// </summary>
        public class seasonData : CallChanged
        {
            private linkData _links;
            private attributeData _attributes;

            public seasonData()
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

            public override string ToString()
            {
                return attributes.title;
            }
        }

        /// <summary>
        /// Contains the company name
        /// </summary>
        public class attributeData : CallChanged
        {
            private string _title;

            public string title {
                get => _title;
                set => SetField(ref _title, value);
            }
        }

        /// <summary>
        /// Contains link data for the next step
        /// </summary>
        public class linkData : CallChanged
        {
            private string _episodes;

            public string episodes {
                get => _episodes;
                set => SetField(ref _episodes, value);
            }
        }


    }
}
