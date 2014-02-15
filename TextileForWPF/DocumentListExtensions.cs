using System.Windows;
using System.Windows.Documents;

namespace leMaik.TextileForWPF {
    public static class DocumentListExtensions {
        public static void AddAtLayer(this List list, Paragraph item, int layer, TextMarkerStyle textmarker) {
            var currentList = list;
            var currentLayer = 1;
            while (currentLayer != layer) {
                if (currentList.ListItems.LastListItem == null ||
                    !(currentList.ListItems.LastListItem.Blocks.LastBlock is List)) {
                    var newList = new List { Margin = new Thickness(0), MarkerStyle = textmarker };
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
