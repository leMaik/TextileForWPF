using System;
using System.Windows;

namespace leMaik.TextileForWPF {
    public class HyperlinkClickedEventArgs : RoutedEventArgs {
        public String Url { get; private set; }

        internal HyperlinkClickedEventArgs(RoutedEvent routedEvent, String url)
            : base(routedEvent) {
            Url = url;
        }
    }

    public delegate void HyperlinkClickedEventHandler(object sender, HyperlinkClickedEventArgs e);
}
