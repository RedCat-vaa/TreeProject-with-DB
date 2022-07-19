using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TreeProject.Models
{
    //Добавление новых данных или редактирование существующих
    public class AttributesComponent
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public interface ITreeComponent
    {
        ObservableCollection<ITreeComponent> Childrens { get; set; }
        ObservableCollection<AttributesComponent> Attributes { get; set; }
        bool IsExpanded { get; set; }
        int IdParent { get; set; }
        int Id { get; set; }
        string Linkname { get; set; }
        string Product { get; set; }
        string TypeProduct { get; set; }
        void Add(ITreeComponent child);
        void Remove(ITreeComponent child);
    }
    public class TreeComponent : ITreeComponent, INotifyPropertyChanged
    {
        public ObservableCollection<ITreeComponent> Childrens { get; set; }
        public ObservableCollection<AttributesComponent> Attributes { get; set; }
        public int IdParent { get; set; } = 0;
        bool isExpanded { get; set; } = true;
        int id { get; set; } = 0;
        string linkname { get; set; }
        string product { get; set; }
        string typeproduct { get; set; }

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }    
        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }
        public string Linkname
        {
            get { return linkname; }
            set
            {
                linkname = value;
                OnPropertyChanged("Linkname");
            }
        }
        public string Product
        {
            get { return product; }
            set
            {
                product = value;
                OnPropertyChanged("Product");
            }
        }
        public string TypeProduct
        {
            get { return typeproduct; }
            set
            {
                typeproduct = value;
                OnPropertyChanged("TypeProduct");
            }
        }
        public TreeComponent(int id = 0, string linkname = "", string product = "", string productType = "")
        {
            Childrens = new ObservableCollection<ITreeComponent>();
            Attributes = new ObservableCollection<AttributesComponent>();
            Id = id;
            Linkname = linkname;
            Product = product;
            TypeProduct = productType;
        }
      
        public void Add(ITreeComponent child)
        {
            if(Childrens!=null)
            {
                Childrens.Add(child);
            }
        }
        public void Remove(ITreeComponent child)
        {
            if (Childrens != null)
            {
                Childrens.Remove(child);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

    }
}
