using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mislint.Components
{
    public sealed partial class Imager : UserControl
    {
        private byte[] _image;

        public Imager(byte[] image)
        {
            this.InitializeComponent();
        }

        private void Imager_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {

        }
    }
}
