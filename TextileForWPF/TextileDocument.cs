using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace leMaik.TextileForWPF {
    public class TextileDocument : FlowDocument {
        #region Regular expressions
        private static readonly Regex listElement = new Regex(@"^(\*+|#+) (.*)", RegexOptions.Compiled);
        private static readonly Regex labeledLink = new Regex(@"""(.+?)(\((.+?)\))?"":(http(s)?://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?)", RegexOptions.Compiled);
        private static readonly Regex link = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?", RegexOptions.Compiled);
        private static readonly Regex image = new Regex(@"!([<>=]+)?(http(s)?://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?)(\((.+?)\))?!", RegexOptions.Compiled);
        #endregion

        public String Textile {
            get { return (String)GetValue(TextileProperty); }
            set { SetValue(TextileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextileProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextileProperty =
            DependencyProperty.Register("Textile", typeof(String), typeof(TextileDocument), new PropertyMetadata(String.Empty, OnTextilePropertyChanged));

        private static void OnTextilePropertyChanged(DependencyObject source,
        DependencyPropertyChangedEventArgs e) {
            TextileDocument td = (TextileDocument)source;
            td.ReparseTextile();
        }

        public static readonly RoutedEvent LinkClickEvent =
            EventManager.RegisterRoutedEvent("LinkClick", RoutingStrategy.Bubble, typeof(HyperlinkClickedEventHandler), typeof(TextileDocument));

        public event HyperlinkClickedEventHandler LinkClick {
            add { AddHandler(LinkClickEvent, value); }
            remove { RemoveHandler(LinkClickEvent, value); }
        }

        /// <summary>
        /// Parst den gesamten Textile-Inhalt dieses Dokuments neu.
        /// </summary>
        private void ReparseTextile() {
#if DEBUG
            DateTime start = DateTime.Now;
#endif
            Section blocks = ParseTextile(Textile);
#if DEBUG
            DateTime stop = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("[TextileForWPF] Parsing took {0} ms (length: {1} characters)", (stop - start).TotalMilliseconds, Textile.Length);
#endif
            this.Blocks.Clear();
            this.Blocks.Add(blocks);
        }

        private Section ParseTextile(String rawTextile) {
            Section r = new Section();
            BlockCollection result = r.Blocks;

            StringBuilder currentParagraph = new StringBuilder();
            List currentList = null;
            bool lastLineWasBlank = true;

            String[] lines = rawTextile.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (String line in lines) {
                if (line.Trim().Length == 0) {
                    if ((result.LastBlock is Paragraph && ((Paragraph)result.LastBlock).Inlines.Count == 0))
                        result.Remove(result.LastBlock);
                    if (currentList != null) {
                        result.Add(currentList);
                        currentList = null;
                    }
                    else if (currentParagraph.Length > 0) {
                        Paragraph p = new Paragraph();
                        p.Inlines.AddRange(ParseParagraph(currentParagraph.ToString(), new TextileFormatCollection()));
                        result.Add(p);
                        currentParagraph.Clear();
                    }
                    lastLineWasBlank = true;
                }
                else {
                    Match listItemMatch = listElement.Match(line);
                    if (listItemMatch.Success) {
                        if (currentParagraph.Length > 0) {
                            Paragraph p = new Paragraph();
                            p.Inlines.AddRange(ParseParagraph(currentParagraph.ToString(), new TextileFormatCollection()));
                            result.Add(p);
                            currentParagraph.Clear();
                        }

                        if (currentList == null) {
                            currentList = new List() { MarkerStyle = listItemMatch.Groups[0].Value[0] == '*' ? TextMarkerStyle.Disc : TextMarkerStyle.Decimal };
                        }
                        Paragraph content = new Paragraph() { TextAlignment = TextAlignment.Left };
                        content.Inlines.AddRange(FormatTextileTo(listItemMatch.Groups[2].Value));
                        currentList.AddAtLayer(content, listItemMatch.Groups[1].Value.Length, listItemMatch.Groups[0].Value[0] == '*' ? TextMarkerStyle.Disc : TextMarkerStyle.Decimal);

                        lastLineWasBlank = false;
                    }
                    else {
                        if (currentList != null) {
                            result.Add(currentList);
                            currentList = null;
                        }
                        if (!lastLineWasBlank)
                            currentParagraph.Append(" ");
                        currentParagraph.Append(line.Trim());
                        if (line.EndsWith("   ")) {
                            currentParagraph.Append(Environment.NewLine);
                            lastLineWasBlank = true;
                        }
                        else {
                            lastLineWasBlank = false;
                        }
                    }
                }
            }

            if ((result.LastBlock is Paragraph && ((Paragraph)result.LastBlock).Inlines.Count == 0))
                result.Remove(result.LastBlock);
            if (currentList != null) {
                result.Add(currentList);
            }
            else {
                Paragraph p = new Paragraph();
                p.Inlines.AddRange(ParseParagraph(currentParagraph.ToString(), new TextileFormatCollection()));
                result.Add(p);
                currentParagraph.Clear();
            }

            return r;
        }

        private List<Inline> ParseParagraph(String textileLine, TextileFormatCollection currentFormat, bool ignoreHyperlinks = false) {
            List<Inline> result = new List<Inline>();

            int i = 0;
            int start = 0;
            int end = 0;
            bool found;
            while (i < textileLine.Length) {
                found = false;
                if (i < textileLine.Length - 1) {
                    if (textileLine[i] == '*' && textileLine[i + 1] == '*') {
                        end = i - 1;
                        if (start <= end)
                            result.Add(MakeRun(textileLine.Substring(start, end - start + 1), currentFormat));
                        i += 2;
                        currentFormat.Flip(TextileFormat.Bold);
                        found = true;
                    }
                    else if (textileLine[i] == '_' && textileLine[i + 1] == '_') {
                        end = i - 1;
                        if (start <= end)
                            result.Add(MakeRun(textileLine.Substring(start, end - start + 1), currentFormat));
                        i += 2;
                        currentFormat.Flip(TextileFormat.Italic);
                        found = true;
                    }
                    else if (textileLine[i] == '+' && textileLine[i + 1] == '+') {
                        end = i - 1;
                        if (start <= end)
                            result.Add(MakeRun(textileLine.Substring(start, end - start + 1), currentFormat));
                        i += 2;
                        currentFormat.Flip(TextileFormat.Underlined);
                        found = true;
                    }
                    else if (textileLine[i] == '-' && textileLine[i + 1] == '-') {
                        end = i - 1;
                        if (start <= end)
                            result.Add(MakeRun(textileLine.Substring(start, end - start + 1), currentFormat));
                        i += 2;
                        currentFormat.Flip(TextileFormat.Stroken);
                        found = true;
                    }
                    else if (!ignoreHyperlinks && textileLine[i] == '"') {
                        Match m = labeledLink.Match(textileLine, i);
                        if (m.Success && m.Index == i) {
                            end = i - 1;
                            result.Add(MakeRun(textileLine.Substring(start, end - start + 1), currentFormat));
                            result.Add(MakeHyperlink(m.Groups[1].Value, m.Groups[4].Value, m.Groups[3].Value, currentFormat));
                            i += m.Value.Length;
                            found = true;
                        }
                    }
                    else if (!ignoreHyperlinks && textileLine[i] == 'h') {
                        Match m = link.Match(textileLine, i);
                        if (m.Success && m.Index == i) {
                            end = i - 1;
                            result.Add(MakeRun(textileLine.Substring(start, end - start + 1), currentFormat));
                            result.Add(MakeHyperlink(m.Value, m.Value, null, currentFormat));
                            i += m.Value.Length;
                            found = true;
                        }
                    }
                    else if (textileLine[i] == '!') {
                        Match m = image.Match(textileLine, i);
                        if (m.Success && m.Index == i) {
                            end = i - 1;
                            result.Add(MakeRun(textileLine.Substring(start, end - start + 1), currentFormat));
                            Image img = new Image() { Source = new BitmapImage(new Uri(m.Groups[2].Value, UriKind.Absolute)) };
                            if (m.Groups[7].Length > 0)
                                img.ToolTip = m.Groups[7].Value;
                            if (m.Groups[1].Value == "<>")
                                img.Stretch = System.Windows.Media.Stretch.Uniform;
                            else
                                img.Stretch = System.Windows.Media.Stretch.None;
                            result.Add(new InlineUIContainer(img));
                            i += m.Value.Length;
                            found = true;
                        }
                    }
                }

                if (!found) {
                    if (textileLine[i] == '~') {
                        end = i - 1;
                        if (start <= end)
                            result.Add(MakeRun(textileLine.Substring(start, end - start + 1), currentFormat));
                        i++;
                        currentFormat.Flip(TextileFormat.Subscript);
                        found = true;
                    }
                    else if (textileLine[i] == '^') {
                        end = i - 1;
                        if (start <= end)
                            result.Add(MakeRun(textileLine.Substring(start, end - start + 1), currentFormat));
                        i++;
                        currentFormat.Flip(TextileFormat.Superscript);
                        found = true;
                    }
                }

                if (found) {
                    start = i;
                }
                else {
                    i++;
                    end++;
                }
            }

            if (textileLine.Length - start > 0) {
                result.Add(MakeRun(textileLine.Substring(start), currentFormat));
            }

            return result;
        }

        private Run MakeRun(String text, TextileFormatCollection format) {
            Run result = new Run(text);
            format.ApplyOn(result);
            return result;
        }

        private Hyperlink MakeHyperlink(String text, String url, String tooltip, TextileFormatCollection format) {
            Hyperlink result = new Hyperlink();

            result.Inlines.AddRange(FormatTextileTo(text, true));

            format.ApplyOn(result);

            if (!String.IsNullOrWhiteSpace(tooltip))
                result.ToolTip = tooltip;

            result.Click += (s, e) => {
                RaiseEvent(new HyperlinkClickedEventArgs(LinkClickEvent, url));
            };
            return result;
        }

        private List<Inline> FormatTextileTo(String textile, bool ignoreHyperlinks = false) {
            return ParseParagraph(textile, new TextileFormatCollection(), ignoreHyperlinks);
        }
    }
}
