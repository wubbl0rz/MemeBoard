using mrousavy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
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
using WpfAnimatedGif;

namespace MemeBoard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Storyboard Storyboard => (Storyboard)this.Resources["imageRotationStoryboard"];

        private MemeRepo memeRepo;
        private List<HotKey> keyBindings = new List<HotKey>();

        private Meme currentMeme;

        private WebInterface webInterface;
        private SpeechSynthesizer tts = new SpeechSynthesizer();
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ToggleImageMeme(Meme meme)
        {
            
            if (this.WindowState != WindowState.Minimized && this.currentMeme == meme)
            {
                this.currentMeme = null;
                this.image.Source = null;
                this.memeRepo.Memes.Where(m => m.Type == MemeType.Image).ToList().ForEach(m => m.Deactivate());
                this.WindowState = WindowState.Minimized;
                return;
            }

            this.ResetAnimation();

            if (meme.IsAnimated)
                this.ShowGif(meme.Path);
            else
                this.ShowBitmap(meme.Path);

            this.currentMeme?.Deactivate();
            meme.Activate();
            this.currentMeme = meme;
            this.ShowActivated = false;
            this.WindowState = WindowState.Normal;
        }

        private void ShowBitmap(string path)
        {
            this.image.Source = new BitmapImage(new Uri(path));
        }

        private void ResetAnimation()
        {
            this.Storyboard.Stop();
            ImageBehavior.SetAnimatedSource(this.image, null);
        }

        private void ShowGif(string path)
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri(path);
            img.EndInit();
            ImageBehavior.SetAnimatedSource(this.image, img);
        }

        private void PlayTextMeme(Meme meme)
        {
            this.tts.SpeakAsyncCancelAll();
            var text = File.ReadAllText(meme.Path);
            meme.Activate();
            
            Task.Run(() => 
            {
                this.tts.Speak(text);
                meme.Deactivate();
                this.webInterface.SendUpdate();
            });
        }

        private void ToggleMeme(Meme meme)
        {
            switch (meme.Type)
            {
                case MemeType.Image:
                    this.ToggleImageMeme(meme);
                    break;
                case MemeType.TTS:
                    this.PlayTextMeme(meme);
                    break;
                default:
                    break;
            }

            this.webInterface.SendUpdate();
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
            var memePath = Path.Combine(Directory.GetCurrentDirectory(), "memes");
            if (!File.Exists(memePath))
                Directory.CreateDirectory(memePath);

            this.memeRepo = new MemeRepo(Path.Combine(Directory.GetCurrentDirectory(), "memes"));

            this.RefreshKeyBindings();

            this.webInterface = new WebInterface(this.memeRepo);
            this.webInterface.MemeClicked += m => this.Dispatcher.Invoke(() => this.ToggleMeme(m));

            this.memeRepo.Updated += () => this.Dispatcher.Invoke(this.RefreshKeyBindings);

            new HotKey(ModifierKeys.Control, Key.PageUp, this, _ => this.Storyboard.Begin());
            new HotKey(ModifierKeys.Control, Key.PageDown, this, _ => this.Storyboard.Stop());

            this.webInterface.Start();
        }
        
        private void TrayExit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TrayStartStopServer(object sender, RoutedEventArgs e)
        {
            if (this.webInterface.IsRunning)
            {
                this.webInterface.Stop();
            }
            else
            {
                this.webInterface.Start();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.trayIcon.Dispose();
        }
    }
}
