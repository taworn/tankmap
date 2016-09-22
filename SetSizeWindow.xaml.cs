using System.Windows;

namespace tankmap {
    /// <summary>
    /// Interaction logic for SetSizeWindow.xaml
    /// </summary>
    public partial class SetSizeWindow : Window {

        private int resultWidth = 0;
        private int resultHeight = 0;

        public SetSizeWindow() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            int w = 0;
            int h = 0;
            bool bw = int.TryParse(textWidth.Text, out w);
            bool bh = int.TryParse(textHeight.Text, out h);
            if (!bw || !bh)
                return;
            if (w < 1 || h < 1)
                return;

            resultWidth = w;
            resultHeight = h;
            DialogResult = true;
        }

        public int GetResultWidth() {
            return resultWidth;
        }

        public int GetResultHeight() {
            return resultHeight;
        }

    }
}
