using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace leMaik.TextileForWPF {
    public static class DocumentListExtensions {
        public static void AddAtLayer(this List list, Paragraph item, int layer, TextMarkerStyle textmarker) {
            List currentList = list;
            int currentLayer = 1;
            while (currentLayer != layer) {
                if (currentList.ListItems.LastListItem == null ||
                    !(currentList.ListItems.LastListItem.Blocks.LastBlock is List)) {
                    List newList = new List() { Margin = new System.Windows.Thickness(0), MarkerStyle = textmarker };
                    if (currentList.ListItems.LastListItem == null)
                        currentList.ListItems.Add(new ListItem());
                    currentList.ListItems.LastListItem.Blocks.Add(newList);
                    currentList = newList;
                }
                else {
                    currentList = (List)currentList.ListItems.LastListItem.Blocks.LastBlock;
                }
                currentLayer++;
            }
            currentList.ListItems.Add(new ListItem(item));
        }
    }
}
