using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace leMaik.TextileForWPF {
    enum TextileFormat {
        Bold,
        Italic,
        Underlined,
        Stroken,
        Superscript,
        Subscript
    }

    class TextileFormatCollection : HashSet<TextileFormat> {
        public void ApplyOn(Inline result) {
            if (Contains(TextileFormat.Bold))
                result.FontWeight = FontWeight.FromOpenTypeWeight(700);
            if (Contains(TextileFormat.Italic))
                result.FontStyle = FontStyles.Italic;
            if (Contains(TextileFormat.Underlined) || Contains(TextileFormat.Stroken)) {
                result.TextDecorations.Clear();
                if (Contains(TextileFormat.Underlined))
                    result.TextDecorations.Add(TextDecorations.Underline);
                if (Contains(TextileFormat.Stroken))
                    result.TextDecorations.Add(TextDecorations.Strikethrough);
            }
            if (Contains(TextileFormat.Subscript))
                result.Typography.Variants = FontVariants.Subscript;
            if (Contains(TextileFormat.Superscript))
                result.Typography.Variants = FontVariants.Superscript;
        }

        new public void Add(TextileFormat t) {
            base.Add(t);

            //Gegensätzliche Formatierungen ggf. entfernen
            switch (t) {
                case TextileFormat.Subscript:
                    Remove(TextileFormat.Superscript);
                    break;
                case TextileFormat.Superscript:
                    Remove(TextileFormat.Subscript);
                    break;
            }
        }

        /// <summary>
        /// Entfernt die Formatierung, falls sie vorhanden ist oder fügt sie hinzu, falls sie noch nicht vorhanden ist.
        /// </summary>
        /// <param name="textileFormat">Eine Formatierung</param>
        public void Flip(TextileFormat textileFormat) {
            if (Contains(textileFormat))
                Remove(textileFormat);
            else
                Add(textileFormat);
        }

        /// <summary>
        /// Kopiert diese TextileFormatCollection und gibt die neue Instanz zurück.
        /// </summary>
        /// <returns>Neue Instanz mit den gleichen Formatierungen</returns>
        internal TextileFormatCollection Clone() {
            TextileFormatCollection result = new TextileFormatCollection();
            foreach (TextileFormat f in this)
                result.Add(f);
            return result;
        }
    }
}
