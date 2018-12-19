using Avalonia.Media.Imaging;
using System.Collections.ObjectModel;

namespace RT_Stream_App.Classes
{

    // API homepage layout
    // [Object array] data
    // [Object] 0,1,2...
    // Show properties


    public class shows : CallChanged
    {
        /// <summary>
        /// Root of the JSON
        /// </summary>
        public class APIData : CallChanged
        {
            private ObservableCollection<showData> _data;

            public APIData()
            {
                this.data = new ObservableCollection<showData>();
            }
            public ObservableCollection<showData> data {
                get => _data;
                set => SetField(ref _data, value);
            }
        }

        /// <summary>
        /// A class that holds the data for each show (Name and link mostly)
        /// </summary>
        public class showData : CallChanged
        {
            private linkData _links;
            private attributeData _attributes;
            private includedData _included;
            private IBitmap _thumbImage;

            public showData()
            {
                this.attributes = new attributeData();
                this.links = new linkData();
            }

            public attributeData attributes
            {
                get => _attributes;
                set => SetField(ref _attributes, value);
            }
            public linkData links
            {
                get => _links;
                set => SetField(ref _links, value);
            }
            public includedData included {
                get => _included;
                set => SetField(ref _included, value);
            }
            public IBitmap thumbImage
            {
                get => _thumbImage;
                set => SetField(ref _thumbImage, value);
            }
        }

        /// <summary>
        /// Contains the show title
        /// </summary>
        public class attributeData : CallChanged
        {
            private string _title;
            private bool _is_sponsors_only;

            public string title
            {
                get => _title;
                set => SetField(ref _title, value);
            }

            public bool is_sponsors_only
            {
                get => _is_sponsors_only;
                set => SetField(ref _is_sponsors_only, value);
            }
        }

        /// <summary>
        /// Contains link data for the next step
        /// </summary>
        public class linkData : CallChanged
        {
            private string _seasons;

            public string seasons {
                get => _seasons;
                set => SetField(ref _seasons, value);
            }
        }

        /// <summary>
        /// Contains the data under 'included', this is mainly images
        /// </summary>
        public class includedData : CallChanged
        {

            public includedData()
            {
                images = new ObservableCollection<imageData>();
            }

            private ObservableCollection<imageData> _images;

            public ObservableCollection<imageData> images {
                get => _images;
                set => SetField(ref _images, value);
            }
        }

        /// <summary>
        /// Contains all of the data for images
        /// </summary>
        public class imageData : CallChanged
        {

            public imageData()
            {
                attributes = new imageAttributes();
            }

            private imageAttributes _attributes;

            public imageAttributes attributes {
                get => _attributes;
                set => SetField(ref _attributes, value);
            }
        }

        /// <summary>
        /// Contains the data for each image set (Different sizes, what it belongs to and what context to use it in)
        /// </summary>
        public class imageAttributes : CallChanged
        {
            private string _thumb;

            public string thumb
            {
                get => _thumb;
                set => SetField(ref _thumb, value);
            }

        }


    }
}
