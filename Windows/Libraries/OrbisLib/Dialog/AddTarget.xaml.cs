using SimpleUI.Dialogs;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OrbisSuite.Dialog
{
    /// <summary>
    /// Interaction logic for AddTarget.xaml
    /// </summary>
    public partial class AddTargetDialog : SimpleDialog
    {
        public AddTargetDialog(Window Owner, WindowStartupLocation StartPosition)
            : base(Owner, "Save", "Cancel", "Add Target", StartPosition)
        {
            InitializeComponent();
        }

        public static SimpleDialogResult ShowDialog(Window Owner, WindowStartupLocation StartPosition)
        {
            var dlg = new AddTargetDialog(Owner, StartPosition);
            dlg.ShowDialog();

            if(dlg.Result == SimpleDialogResult.Button1)
            {
                Console.WriteLine("Save");
            }

            return dlg.Result;
        }
    }
}
