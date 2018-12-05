﻿using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;

namespace RT_Stream_App.Classes
{

    // API homepage layout
    // [Object array] data
        // [Object] 0,1,2...
            // Show properties


    public class episodes : CallChanged
    {
        /// <summary>
        /// Root of the JSON
        /// </summary>
        public class APIData : CallChanged
        {
            private ObservableCollection<episodeData> _data;

            public APIData()
            {
                this.data = new ObservableCollection<episodeData>();
            }
            public ObservableCollection<episodeData> data {
                get => _data;
                set => SetField(ref _data, value);
            }
        }

        /// <summary>
        /// A class that holds the data for each show (Name and link mostly)
        /// </summary>
        public class episodeData : CallChanged
        {
            private linkData _links;
            private attributeData _attributes;
            private includedData _included;
            private IBitmap _thumbImage;

            public episodeData()
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
            private string _caption;
            private DateTime _public_golive_at;
            private DateTime _sponsor_golive_at;
            private DateTime _member_golive_at;

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

            public string caption
            {
                get => _caption;
                set => SetField(ref _caption, value);
            }

            public DateTime public_golive_at
            {
                get => _public_golive_at;
                set => SetField(ref _public_golive_at, value);
            }

            public DateTime sponsor_golive_at
            {
                get => _sponsor_golive_at;
                set => SetField(ref _sponsor_golive_at, value);
            }

            public DateTime member_golive_at
            {
                get => _member_golive_at;
                set => SetField(ref _member_golive_at, value);
            }
        }

        /// <summary>
        /// Contains link data for the next step
        /// </summary>
        public class linkData : CallChanged
        {
            private string _videos;

            public string videos {
                get => _videos;
                set => SetField(ref _videos, value);
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
