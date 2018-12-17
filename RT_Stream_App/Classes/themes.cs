using Avalonia.Media;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RT_Stream_App.Classes
{
    public class themes: CallChanged
    {

        public themes(string fore, string back, string name)
        {
            foreground = fore;
            background = back;
            this.name = name;
        }

        private string _foreground;
        private string _background;
        private string _name;

        public string foreground { get => _foreground; set => SetField( ref _foreground, value); }
        [JsonIgnore]
        public Brush foregroundCol { get => (Brush)Brush.Parse(_foreground);}
        public string background { get => _background; set => SetField( ref _background, value); }
        [JsonIgnore]
        public Brush backgroundCol { get => (Brush)Brush.Parse(_background);}
        public string name { get => _name; set => SetField( ref _name, value); }

        public override string ToString()
        {
            return name;
        }
    }
}
