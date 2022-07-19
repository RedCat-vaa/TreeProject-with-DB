using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TreeProject.Views;
using TreeProject.Models;

namespace TreeProject.ViewModels
{
    public class EditVM : INotifyPropertyChanged
    {
        IMyDatabase database;
        public TypeSave typesave { get; set; }
        public ComponentForViewModels ComponentsForVM {get;set;}
        public Edit EditWindow { get; set; }
        private int idparent { get; set; } = 0;
        private string typeProduct { get; set; } = String.Empty;
        private string product { get; set; } = String.Empty;
        private string linkname { get; set; } = String.Empty;

        public ObservableCollection<AttributesComponent> attributesList;

        public int IdParent
        {
            get { return idparent; }
            set
            {
                idparent = value;
                OnPropertyChanged("IdParent");
            }
        }
        public string TypeProduct
        {
            get { return typeProduct; }
            set
            {
                typeProduct = value;
                OnPropertyChanged("TypeProduct");
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
        public string LinkMame
        {
            get { return linkname; }
            set
            {
                linkname = value;
                OnPropertyChanged("LinkMame");
            }
        }

        public EditVM(ComponentForViewModels ComponentsForVM, TypeSave typesave, Edit EditWindow, IMyDatabase database)
        {
            this.ComponentsForVM = ComponentsForVM;
            this.EditWindow = EditWindow;
            IdParent = 0;
            this.typesave = typesave;
            this.database = database;
            if (typesave == TypeSave.Edit)
            {
                IdParent = ComponentsForVM.SelectedComponentVM.IdParent;
                LinkMame = ComponentsForVM.SelectedComponentVM?.Linkname;
                Product = ComponentsForVM.SelectedComponentVM?.Product;
                TypeProduct = ComponentsForVM.SelectedComponentVM?.TypeProduct;
                attributesList = new ObservableCollection<AttributesComponent>(ComponentsForVM.SelectedComponentVM?.Attributes);
            }
            else
            {
                attributesList = new ObservableCollection<AttributesComponent>();
            }
        }

        //Команда для добавления, редактирования
        private Command _Save;
        public Command Save
        {
            get
            {
                return _Save = _Save ?? new Command(SaveHandler, () => true);
            }
        }

        //Обработчик добавления, редактирования
        public void SaveHandler(object parameter)
        {
          
            ITreeComponent parentComponent = null;
            ITreeComponent treeComponent = null;

            ITreeComponent findId(ObservableCollection<ITreeComponent> components, int? parentID)
            {
                ITreeComponent findComponent;
                foreach (ITreeComponent treecomponent in components)
                {
                    if (treecomponent.Id == parentID)
                    {
                        return treecomponent;
                    }
                    if (treecomponent.Childrens.Count() > 0)
                    {
                        findComponent = findId(treecomponent.Childrens, parentID);
                        if (findComponent != null)
                        {
                            return findComponent;
                        }
                    }
                }
                return null;
            }

            if (IdParent > 0)
            {
                parentComponent = findId(ComponentsForVM.AllComponentsVM, IdParent);
                if (parentComponent==null)
                {
                    IdParent = 0;
                }
            }
            if (!MainVM.startDB)
            {
                database.CheckTables();
                MainVM.startDB = true;
            }
            if (typesave == TypeSave.Edit)
            {
                treeComponent = ComponentsForVM.SelectedComponentVM;
                if (treeComponent.Id== (int)IdParent)
                {
                    IdParent = 0;
                }
                treeComponent.Product = Product;
                treeComponent.TypeProduct = TypeProduct;
                treeComponent.IdParent = (int)IdParent;
                treeComponent.Linkname = LinkMame;
                treeComponent.Attributes = attributesList;
                database.EditData(treeComponent);
            }
            else
            {
                treeComponent = new TreeComponent(0, LinkMame, Product, TypeProduct) { IdParent = (int)IdParent, Attributes = attributesList };
                database.AddData(treeComponent);
            }
            ComponentsForVM.ChangeData = true;
            EditWindow?.Close(); 
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
