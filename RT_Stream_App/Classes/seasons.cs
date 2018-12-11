﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace RT_Stream_App.Classes
{

    // API homepage layout
    // [Object array] data
        // [Object] 0,1,2...
            // Show properties


    public class seasons : baseClass
    {
        /// <summary>
        /// Root of the JSON
        /// </summary>
        public new class APIData : baseClass.APIData
        {
            private ObservableCollection<seasonData> _data;

            public APIData()
            {
                this.data = new ObservableCollection<seasonData>();
            }
            public new ObservableCollection<seasonData> data
            {
                get => _data;
                set => SetField(ref _data, value);
            }
        }

        /// <summary>
        /// A class that holds the data for each company (Name and link mostly)
        /// </summary>
        public class seasonData : objectData
        {
            private linkData _links;
            private attributeData _attributes;

            public seasonData()
            {
                this.attributes = new attributeData();
                this.links = new linkData();
            }

            public new attributeData attributes {
                get => _attributes;
                set => SetField(ref _attributes, value);
            }
            public new linkData links
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
        public new class attributeData : baseClass.attributeData
        {
            private string _title;

            public string title {
                get => displayText;
                set => displayText = value;
            }
        }

        /// <summary>
        /// Contains link data for the next step
        /// </summary>
        public new class linkData : baseClass.linkData
        {
            public string episodes {
                get => nextLink.Substring(0, nextLink.IndexOf('?'));
                set => nextLink = value;
            }
        }


    }
}
