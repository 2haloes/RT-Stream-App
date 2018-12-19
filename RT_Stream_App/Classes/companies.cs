using System.Collections.ObjectModel;

namespace RT_Stream_App.Classes
{

    // API homepage layout
    // [Object array] data
    // [Object] 0,1,2...
    // Show properties


    public class companies : CallChanged
    {
        /// <summary>
        /// Root of the JSON
        /// </summary>
        public class APIData : CallChanged
        {
            private ObservableCollection<companyData> _data;

            public APIData()
            {
                this.data = new ObservableCollection<companyData>();
            }
            public ObservableCollection<companyData> data {
                get => _data;
                set => SetField(ref _data, value);
            }
        }

        /// <summary>
        /// A class that holds the data for each company (Name and link mostly)
        /// </summary>
        public class companyData : CallChanged
        {
            private linkData _links;
            private attributeData _attributes;

            public companyData()
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
            private string _name;

            public string name {
                get => _name;
                set => SetField(ref _name, value);
            }
        }

        /// <summary>
        /// Contains link data for the next step
        /// </summary>
        public class linkData : CallChanged
        {
            private string _shows;

            public string shows {
                get => _shows;
                set => SetField(ref _shows, value);
            }
        }


    }
}
