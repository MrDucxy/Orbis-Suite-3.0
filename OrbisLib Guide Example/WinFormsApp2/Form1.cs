using OrbisLib2;
using OrbisLib2.Targets;

namespace WinFormsApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void targetListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            TargetManager.SelectedTarget = TargetManager.GetTarget(targetListBox.GetItemText(targetListBox.SelectedItem));
            MessageBox.Show(TargetManager.SelectedTarget.Name);
        }

        private void refreshTargetsBtn_Click(object sender, EventArgs e)
        {
            targetListBox.Items.Clear();
            foreach(Target target in TargetManager.Targets)
            {
                targetListBox.Items.Add(target.Name);
            }
        }
    }
}