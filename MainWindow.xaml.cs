using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Deployment.Compression.Cab;

namespace CabiNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        private string _sourcePath;
        private string _targetPath;
        private string _archiveName;
        private bool _isBusy;
        private bool _isSuccess;
        private bool _includeSubFolders;

        public event PropertyChangedEventHandler PropertyChanged;

        public string SourcePath
        {
            get { return _sourcePath; }
            set
            {
                if (value != _sourcePath)
                {
                    _sourcePath = value;
                    OnPropertyChanged("SourcePath");
                }
            }
        }

        public string TargetPath
        {
            get { return _targetPath; }
            set
            {
                if (value != _targetPath)
                {
                    _targetPath = value;
                    OnPropertyChanged("TargetPath");
                }
            }
        }

        public string ArchiveName
        {
            get { return _archiveName; }
            set
            {
                if (value != _archiveName)
                {
                    _archiveName = (value.EndsWith(".cab")) ? value : $"{value}.cab";
                    OnPropertyChanged("ArchiveName");
                }
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    OnPropertyChanged("IsBusy");
                }
            }
        }

        private void Browse_Source_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();

            SourcePath = dialog.SelectedPath;
        }

        private void Browse_Target_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();

            TargetPath = dialog.SelectedPath;
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            IsBusy = true;
            var archive = Path.Combine(TargetPath, ArchiveName);

            try
            {
                // Create the file, or overwrite if the file exists.
                File.Create(archive).Dispose();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    var cabInfo = new CabInfo(archive);
                    cabInfo.Pack(SourcePath, _includeSubFolders, Microsoft.Deployment.Compression.CompressionLevel.Max, null);
                    _isSuccess = true;
                }
                catch
                {
                    _isSuccess = false;
                }
            });

            IsBusy = false;

            if (_isSuccess)
            {
                System.Windows.MessageBox.Show("Successfully created cabinet!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show("Oops something went wrong!", "Failure", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Subfolder_Checked(object sender, RoutedEventArgs e)
        {
            _includeSubFolders = true;
        }

        private void Subfolder_Unchecked(object sender, RoutedEventArgs e)
        {
            _includeSubFolders = false;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !string.IsNullOrEmpty(SourcePath) && !string.IsNullOrEmpty(TargetPath) && !string.IsNullOrEmpty(ArchiveName) && !IsBusy;
        }
    }
}