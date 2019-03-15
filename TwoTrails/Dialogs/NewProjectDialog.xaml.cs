using System;
using System.Windows;
using System.IO;
using TwoTrails.Core;
using System.ComponentModel;
using FMSC.Core.Windows.Utilities;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for NewProjectDialog.xaml
    /// </summary>
    public partial class NewProjectDialog : Window
    {
        public TtProjectInfo ProjectInfo { get; private set; }

        public String FilePath { get; private set; }

        public NewProjectDialog(Window owner, TtProjectInfo projectInfo, String defaultDir = null)
        {
            this.Owner = owner;
            ProjectInfo = new TtProjectInfo(projectInfo);
            InitializeComponent();
            prjInfoCtrl.SetProjectInfo(ProjectInfo);

            txtLocation.Text = defaultDir ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            ProjectInfo.PropertyChanged += ProjectInfo_PropertyChanged;
        }

        private void ProjectInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Name", StringComparison.InvariantCultureIgnoreCase))
            {
                txtName.Text = ProjectInfo.Name.Replace(' ', '_');
            }
        }


        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtLocation.Text = dialog.SelectedPath;
                }
            }
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            bool result = false;
            try
            {
                if (String.IsNullOrWhiteSpace(ProjectInfo.Name))
                {
                    MessageBox.Show("Project must have a name.");
                    prjInfoCtrl.FocusName();
                }
                else if (String.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Must have a Filename.");
                    txtName.Focus();
                }
                else
                {
                    String fileName = txtName.Text;

                    if (!fileName.EndsWith(Consts.FILE_EXTENSION))
                        fileName = $"{fileName}{Consts.FILE_EXTENSION}";

                    if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
                    {
                        FilePath = Path.Combine(txtLocation.Text, fileName);

                        if (File.Exists(FilePath))
                        {
                            if (MessageBox.Show($"{fileName} already exists. Would you like to overwrite it?", "File Exists",
                                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            {
                                result = true;
                            }
                        }
                        else
                            result = true;

                        if (result == true)
                        {
                            if (this.IsShownAsDialog())
                                this.DialogResult = true;
                            this.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid File Name.");
                        txtName.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsShownAsDialog())
                this.DialogResult = false;
            this.Close();
        }
    }
}
