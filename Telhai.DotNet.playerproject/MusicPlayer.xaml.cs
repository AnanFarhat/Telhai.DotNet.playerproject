
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Telhai.DotNet.playerproject;
using Telhai.DotNet.PlayerProject.Models;
using Telhai.DotNet.PlayerProject.Services;

namespace Telhai.DotNet.PlayerProject
{
    /// <summary>
    /// Interaction logic for MusicPlayer.xaml
    /// </summary>
    public partial class MusicPlayer : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private DispatcherTimer timer = new DispatcherTimer();
        private List<MusicTrack> library = new List<MusicTrack>();
        private bool isDragging = false;
        private const string FILE_NAME = "library.json";
        private readonly ItunesService _itunesService = new ItunesService();
        private CancellationTokenSource? _itunesCts;
        private readonly MetadataStorageService _storage = new MetadataStorageService();



        public MusicPlayer()
        {
            InitializeComponent();//init all hardcoded xaml into Object tree
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += new EventHandler(Timer_Tick);
            
            this.Loaded += MusicPlayer_Loaded ;
            SetDefaultCover();


            txtArtist.Text = "-";
            txtAlbum.Text = "-";
            txtFilePath.Text = "";
        }

        private void MusicPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadLibrary();        }

        private void LoadLibrary()
        {
            if (File.Exists(FILE_NAME))
            {
                string json = File.ReadAllText(FILE_NAME);
                library = JsonSerializer.Deserialize<List<MusicTrack>>(json) ?? new List<MusicTrack>();
                UpdateLibraryUI();
            }
        }


        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan && !isDragging)
            {
                sliderProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliderProgress.Value = mediaPlayer.Position.TotalSeconds;

            }
        }

        // --- EMPTY PLACEHOLDERS TO MAKE IT BUILD ---
        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            PlaySelectedTrack();
        }




        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            txtStatus.Text = "Paused";
        }


        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            timer.Stop();
            sliderProgress.Value = 0;
            txtStatus.Text = "Stopped";
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = sliderVolume.Value;
        }




        private void Slider_DragStarted(object sender, MouseButtonEventArgs e)
        {
            isDragging = true; // Stop timer updates
        }
        private void Slider_DragCompleted(object sender, MouseButtonEventArgs e)
        {
            isDragging = false; // Resume timer updates
            mediaPlayer.Position = TimeSpan.FromSeconds(sliderProgress.Value);
        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "MP3 Files|*.mp3";

            if (ofd.ShowDialog() == true)
            {
                foreach (string file in ofd.FileNames)
                {
                    MusicTrack track = new MusicTrack
                    {
                        Title = System.IO.Path.GetFileNameWithoutExtension(file),
                        FilePath = file
                    };
                    library.Add(track);
                }
                UpdateLibraryUI();
                SaveLibrary();
            }
        }

        private void SaveLibrary()
        {
            var options =new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(library,options);
            File.WriteAllText(FILE_NAME, json);
        }

        private void UpdateLibraryUI()
        {
            lstLibrary.ItemsSource = null;
            lstLibrary.ItemsSource = library;      }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                library.Remove(track);
                UpdateLibraryUI();
                SaveLibrary();
            }
        }
        private void LstLibrary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlaySelectedTrack();
        }
        private void PlaySelectedTrack()
        {
            if (lstLibrary.SelectedItem is not MusicTrack track)
                return;

            mediaPlayer.Open(new Uri(track.FilePath));
            mediaPlayer.Play();
            timer.Start();

            SetDefaultMetadata(track);   // resets artist/album + default cover
            SetPlayingStatus(track);     // sets status to Playing...

            _ = LoadItunesMetadataAsync(track); // starts API call immediately
        }
       
        private void SetPlayingStatus(MusicTrack track)
        {
            
            txtCurrentSong.Text = track.Title;
            txtFilePath.Text = track.FilePath;
            txtStatus.Text = "Playing...";
        }


        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            //1 create the settings window instance
            Settings settingsWin = new Settings();

            // 2 subscribe/register to the OnScanCompleted event
            settingsWin.OnScanCompleted += SettingsWin_OnScanCompleted;
            
            settingsWin.ShowDialog();
        }


        private void SettingsWin_OnScanCompleted(List<MusicTrack> eventDateList)
        {
            foreach (var track in eventDateList)
            {
                // Prevent duplicates based on FilePath
                if (!library.Any(x => x.FilePath == track.FilePath))
                {
                    library.Add(track);
                }
            }

            UpdateLibraryUI();
            SaveLibrary();
        }
        private void LstLibrary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstLibrary.SelectedItem is not MusicTrack track)
                return;

            // Show local info immediately
            SetDefaultMetadata(track);

            // Try to load cached metadata from JSON
            ShowMetadataFromJsonIfExists(track);
        }
        private void ShowMetadataFromJsonIfExists(MusicTrack track)
        {
            var allData = _storage.LoadAll();

            if (!allData.TryGetValue(track.FilePath, out var saved))
                return;

            txtArtist.Text = saved.Artist ?? "-";
            txtAlbum.Text = saved.Album ?? "-";

            if (!string.IsNullOrEmpty(saved.ApiArtworkUrl))
                imgCover.Source = new BitmapImage(new Uri(saved.ApiArtworkUrl));

            txtStatus.Text = "Loaded from JSON cache 💾";
        }



        private string BuildSearchTermFromFile(string filePath)
        {
            // file name without extension
            string name = System.IO.Path.GetFileNameWithoutExtension(filePath) ?? "";

            // teacher: separated by spaces or hyphen -> normalize to spaces
            name = name.Replace("-", " ").Replace("_", " ");

            // optional: remove extra spaces
            name = string.Join(" ", name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            return name;
        }

        private void SetDefaultMetadata(MusicTrack track)
        {
            txtCurrentSong.Text = track.Title;
            txtFilePath.Text = track.FilePath;

            txtArtist.Text = "-";
            txtAlbum.Text = "-";

            SetDefaultCover();   // important
            txtStatus.Text = "Ready";
        }
        private void SetDefaultCover()
        {
            imgCover.Source = new BitmapImage(
                new Uri("pack://application:,,,/Assets/default_cover.jpg")
            );
        }

        private async Task LoadItunesMetadataAsync(MusicTrack track)
        {
            // cancel previous request
            _itunesCts?.Cancel();
            _itunesCts = new CancellationTokenSource();

            try
            {
                txtStatus.Text = "Loading from iTunes API 🌐...";


                string term = BuildSearchTermFromFile(track.FilePath);

                var info = await _itunesService.SearchOneAsync(term, _itunesCts.Token);

                if (info == null)
                {
                    txtStatus.Text = "No iTunes result.";
                    return;
                }
                // ✅ save to JSON cache (key = FilePath)
                _storage.Save(track.FilePath, new SongMetadata
                {
                    Artist = info.ArtistName ?? "-",
                    Album = info.AlbumName ?? "-",
                    ApiArtworkUrl = info.ArtworkUrl,
                    TrackName = info.TrackName ?? track.Title
                });


                // update UI
                txtCurrentSong.Text = info.TrackName ?? track.Title;
                txtArtist.Text = info.ArtistName ?? "-";
                txtAlbum.Text = info.AlbumName ?? "-";
                txtFilePath.Text = track.FilePath;

                // load cover from URL (if exists)
                if (!string.IsNullOrWhiteSpace(info.ArtworkUrl))
                {
                    var bmp = new System.Windows.Media.Imaging.BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(info.ArtworkUrl, UriKind.Absolute);
                    bmp.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();

                    imgCover.Source = bmp;
                }

                txtStatus.Text = "Info loaded.";
            }
            catch (OperationCanceledException)
            {
                // user switched songs quickly - ignore
            }
            catch
            {
                // teacher fallback: show filename without extension + full path
                txtCurrentSong.Text = System.IO.Path.GetFileNameWithoutExtension(track.FilePath);
                txtFilePath.Text = track.FilePath;
                txtArtist.Text = "-";
                txtAlbum.Text = "-";
                txtStatus.Text = "API error (showing local info).";
                // keep default cover
            }
        }
        private void LstLibrary_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(lstLibrary, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
                lstLibrary.SelectedItem = item.DataContext;
        }



    }
}
