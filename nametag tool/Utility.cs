using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace nametag_tool
{
    static class Utility
    {
        public static ObservableCollection<NameDataModel> SortNameCollection(ObservableCollection<NameDataModel> collection, OrderBy orderBy)
        {
            ObservableCollection<NameDataModel> temp = collection;

            switch (orderBy)
            {
                case OrderBy.nameAsc:
                    temp = new ObservableCollection<NameDataModel>(collection.OrderBy(p => p.Name));
                    break;
                case OrderBy.nameDesc:
                    temp = new ObservableCollection<NameDataModel>(collection.OrderByDescending(p => p.Name));
                    break;
                case OrderBy.nameLengthAsc:
                    temp = new ObservableCollection<NameDataModel>(collection.OrderBy(p => p.Name.Length));
                    break;
                case OrderBy.nameLengthDesc:
                    temp = new ObservableCollection<NameDataModel>(collection.OrderByDescending(p => p.Name.Length));
                    break;
            }

            collection.Clear();
            foreach (NameDataModel j in temp) collection.Add(j);
            return collection;
        }
        public static childItem FindVisualChild<childItem>(DependencyObject obj)
        where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }
}
