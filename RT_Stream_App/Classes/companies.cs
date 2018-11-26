using System;
using System.Collections.Generic;
using System.Text;

namespace RT_Stream_App.Classes
{

    // API homepage layout
    // [Object array] data
        // [Object] 0,1,2...
            // Show properties


    public class companies
    {
        /// <summary>
        /// Root of the JSON
        /// </summary>
        public class APIData
        {
            public List<companyData> data = new List<companyData>();
            public List<companyData> dataTrimmed;
        }

        /// <summary>
        /// A class that holds the data for each company (Name and link mostly)
        /// </summary>
        public class companyData
        {
            public attributeData attributes = new attributeData();
            public linkData links = new linkData();
        }

        /// <summary>
        /// Contains the company name
        /// </summary>
        public class attributeData
        {
            public string name;
        }

        /// <summary>
        /// Contains link data for the next step
        /// </summary>
        public class linkData
        {
            public string shows;
        }
    }
}
