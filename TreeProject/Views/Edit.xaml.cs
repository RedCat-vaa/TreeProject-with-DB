using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TreeProject.ViewModels;
using TreeProject.Models;

namespace TreeProject.Views
{
    /// <summary>
    /// Логика взаимодействия для Edit.xaml
    /// </summary>
    public partial class Edit : Window
    {
        ComponentForViewModels ComponentsForVM;
        TypeSave typeEdit;
        IMyDatabase database;
        public Edit()
        {
            InitializeComponent();
        }

        public Edit(ComponentForViewModels ComponentsForVM, TypeSave edit, IMyDatabase database)
        {
            InitializeComponent();
            this.ComponentsForVM = ComponentsForVM;
            typeEdit = edit;
            this.database = database;
            this.Loaded += WindowLoaded;
            
        }

        public void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (typeEdit == TypeSave.Edit) this.Title = "Редактировать";
            else if (typeEdit == TypeSave.Add ) this.Title = "Добавить";
            EditVM vmEdit = new EditVM(ComponentsForVM, typeEdit, this, database);
            this.DataContext = vmEdit;
            AttributesGrid.ItemsSource = vmEdit.attributesList;
        }
    }
}
