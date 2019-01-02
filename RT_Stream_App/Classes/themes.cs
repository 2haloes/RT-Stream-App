using Avalonia.Media;
using Newtonsoft.Json;

namespace RT_Stream_App.Classes
{
    public class themes: CallChanged
    {

        public themes(string fore, string back, string stackBack, string name)
        {
            foreground = fore;
            background = back;
            stackBackground = stackBack;
            this.name = name;
        }

        private string _foreground;
        private string _background;
        private string _stackBackground;
        private string _name;

        public string foreground { get => _foreground; set => SetField( ref _foreground, value); }
        [JsonIgnore]
        public IBrush foregroundCol { get => Brush.Parse(_foreground);}
        public string background { get => _background; set => SetField( ref _background, value); }
        [JsonIgnore]
        public IBrush backgroundCol { get => Brush.Parse(_background);}
        public string stackBackground { get => _stackBackground; set => SetField(ref _stackBackground, value); }
        [JsonIgnore]
        public IBrush stackBackgroundCol { get => Brush.Parse(_stackBackground); }
        public string name { get => _name; set => SetField( ref _name, value); }
        [JsonIgnore]
        public IBrush refreshBackgroudnCol { get => Brush.Parse("white"); }

        public override string ToString()
        {
            return name;
        }
    }
}
