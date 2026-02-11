using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace Telhai.DotNet.PlayerProject
{
    public class EditSongViewModel : INotifyPropertyChanged
    {
        private string _filePath = "";
        private string _title = "";
        private string _artist = "-";
        private string _album = "-";
        private BitmapImage? _coverImage;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FilePath
        {
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public string Artist
        {
            get => _artist;
            set { _artist = value; OnPropertyChanged(); }
        }

        public string Album
        {
            get => _album;
            set { _album = value; OnPropertyChanged(); }
        }

        // עטיפה לתצוגה בחלון עריכה (מה-JSON בלבד)
        public BitmapImage? CoverImage
        {
            get => _coverImage;
            set { _coverImage = value; OnPropertyChanged(); }
        }

        // אוסף תמונות לשיר (נוסיף/נמחק בחלון)
        public ObservableCollection<string> ImagePaths { get; } = new ObservableCollection<string>();

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
