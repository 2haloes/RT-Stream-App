using Avalonia;
using Avalonia.Markup.Xaml;

namespace RT_Stream_App
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
