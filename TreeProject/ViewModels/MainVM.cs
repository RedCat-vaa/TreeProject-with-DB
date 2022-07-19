using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeProject.Models;
using TreeProject.Views;
using System.IO;

namespace TreeProject.ViewModels
{
   public enum TypeSave
    {
        Add,
        Edit
    }
    public class ComponentForViewModels
    {
        public ObservableCollection<ITreeComponent> AllComponentsVM { get; set; }
        public ITreeComponent SelectedComponentVM { get; set; }
        public bool ChangeData { get; set; }
    }

    //Контекст для главного окна
    public class MainVM
    {
        public ObservableCollection<ITreeComponent> Components;
        public ITreeComponent SelectedComponent;
        IMyDatabase database;
        public static bool startDB = false;

        public MainVM(IMyDatabase database)
        {
            this.database = database;
            Components = new ObservableCollection<ITreeComponent>();
        }

        //Команда для добавления, редактирования, удаления данных
        private Command _TreeEdit;
        public Command TreeEdit
        {
            get
            {
                return _TreeEdit = _TreeEdit ?? new Command(TreeEditHandler, () => true);
            }
        }

        //Обработчик добавления, редактирования, удаления данных
        public void TreeEditHandler(object parameter)
        {
            bool changeData = false;
            string TypeHadler = parameter as string;
            if (TypeHadler == "add"|| TypeHadler == "edit")
            {
                ComponentForViewModels compVM = new ComponentForViewModels()
                { AllComponentsVM = Components, SelectedComponentVM = SelectedComponent, ChangeData = false };

                Edit editWindow;
                if (TypeHadler == "add")
                {
                    editWindow  = new Edit(compVM, TypeSave.Add, database);
                }
                else
                {
                    editWindow = new Edit(compVM, TypeSave.Edit, database);
                }
               
                editWindow.ShowDialog();
                changeData = compVM.ChangeData;
            }
            else if (TypeHadler == "remove")
            {
                if (SelectedComponent != null && database != null)
                {
                    database.RemoveData(SelectedComponent);
                }
                changeData = true;
            }
            if (changeData)
            {
                UpdateFormDB();
            }        
        }

        //Команда для получения данных из базы данных
        private Command _GetDB;
        public Command GetDB
        {
            get
            {
                return _GetDB = _GetDB ?? new Command(GetDBHandler, () => true);
            }
        }

        //Обработчик для получения данных из базы данных
        private void UpdateFormDB()
        {
            if (!startDB)
            {
                database.CheckTables();
                startDB = true;
            }
            ObservableCollection<ITreeComponent> componentsDB;
            Components.Clear();
            componentsDB = database.GetData();
            var rootComponents = componentsDB.Where(c => c.IdParent == 0);
            foreach (var parentComponent in rootComponents)
            {
                Components.Add(parentComponent);
                findId(parentComponent);
            }

            void findId(ITreeComponent comp)
            {
                var childcomponents = componentsDB.Where(c => c.IdParent == comp.Id);
                if (childcomponents.Count() > 0)
                {
                    foreach (var child in childcomponents)
                    {
                        comp.Add(child);
                        findId(child);
                    }
                }
            }
        }
        public void GetDBHandler(object parametr)
        {        
            UpdateFormDB();
        }
    }
}
