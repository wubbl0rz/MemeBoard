using mrousavy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace MemeBoard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Storyboard Storyboard => (Storyboard)this.Resources["imageRotationStoryboard"];

        private MemeRepo memeRepo = new MemeRepo(@"C:\Users\stream\Desktop\memes2");
        private List<HotKey> keyBindings = new List<HotKey>();

        private Meme currentMeme = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ToggleMeme(Meme meme)
        {
            if (this.IsVisible && this.currentMeme == meme)
            {
                this.Hide();
                return;
            }

            Storyboard.Stop();
            ImageBehavior.SetAnimatedSource(this.image, null);

            if (meme.IsAnimated)
            {
                var img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri(meme.Path);
                img.EndInit();
                ImageBehavior.SetAnimatedSource(image, img);
            }
            else
            {
                this.image.Source = new BitmapImage(new Uri(meme.Path));
            }

            this.currentMeme = meme;

            this.ShowActivated = false;
            this.Show();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            Native.SetWindowExTransparent(hwnd);
        }

        private void RefreshKeyBindings()
        {
            this.keyBindings.ForEach(k => k.Dispose());
            this.keyBindings.Clear();
            
            foreach (var meme in this.memeRepo.Memes)
            {
                if (Enum.TryParse<Key>(meme.Prefix, true, out var result))
                    this.keyBindings.Add(new HotKey(ModifierKeys.Control, 
                        result, this, _ => this.ToggleMeme(meme)));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.RefreshKeyBindings();

            this.memeRepo.Updated += () => this.Dispatcher.Invoke(this.RefreshKeyBindings);

            new HotKey(ModifierKeys.Control, Key.PageUp, this, _ => this.Storyboard.Begin());
            new HotKey(ModifierKeys.Control, Key.PageDown, this, _ => this.Storyboard.Stop());
        }
        
        private void TrayExit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
