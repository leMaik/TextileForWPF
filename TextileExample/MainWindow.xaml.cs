using System.Windows;

namespace leMaik.TextileForWPF.Example {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void TextileDocument_LinkClick(object sender, HyperlinkClickedEventArgs e) {
            MessageBox.Show(e.Url);
        }
    }
}
