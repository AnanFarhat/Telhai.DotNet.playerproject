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
using Telhai.DotNet.PlayerProject.Services;

namespace Telhai.DotNet.PlayerProject
{
    /// <summary>
    /// Interaction logic for EditSongWindow.xaml
    /// </summary>
    public partial class EditSongWindow : Window
    {
        public EditSongViewModel VM { get; }

        public EditSongWindow(EditSongViewModel vm)
        {
            InitializeComponent();
            VM = vm;
            DataContext = VM;
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.png;*.jpeg";
            dialog.Multiselect = true;

            if (dialog.ShowDialog() == true)
            {
                foreach (var file in dialog.FileNames)
                {
                    VM.ImagePaths.Add(file);
                }
            }
        }
        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (lstImages.SelectedItem is string selected)
            {
                VM.ImagePaths.Remove(selected);
            }
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var storage = new MetadataStorageService();
            var all = storage.LoadAll();

            if (!all.TryGetValue(VM.FilePath, out var meta))
            {
                meta = new SongMetadata
                {
                    // אם אין בכלל מטא קיים, נשאיר Artist/Album כמו שיש ב-VM
                    Artist = VM.Artist,
                    Album = VM.Album
                };
            }

            // לא נוגעים ב-ApiArtworkUrl אם כבר קיים
            meta.TrackName = VM.Title;

            // רשימת תמונות חדשה מה-VM
            meta.Images = VM.ImagePaths.ToList();

            all[VM.FilePath] = meta;
            storage.SaveAll(all);


            MessageBox.Show("Saved to JSON ✅");
            Close();
        }


    }

}
