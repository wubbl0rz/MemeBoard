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
        private Storyboard sb => (Storyboard)this.Resources["imageRotationStoryboard"];
        private bool isRotating = false;

        private List<Meme> memes = new List<Meme>();
        private List<HotKey> keyBindings = new List<HotKey>();

        private Meme currentMeme = null;

        public MainWindow()
        {
            InitializeComponent();

            var path = @"C:\Users\stream\Desktop\memes2";
            this.memes = Meme.Load(path);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //TODO: pageup pagedown 
            if (this.isRotating)
            {
                sb.Stop();
                this.isRotating = false;
            }
            else
            {
                sb.Begin();
                this.isRotating = true;
            }
        }

        private void ChangeMeme(Meme meme)
        {
            if (this.IsVisible && this.currentMeme == meme)
            {
                this.Hide();
                return;
            }

            sb.Stop();
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

        private void SetKeyBindings(Meme meme)
        {
            Enum.TryParse<Key>(meme.BindingKey, out var result);

            var key = new HotKey(ModifierKeys.Control, result, this, _ => this.ChangeMeme(meme));

            this.keyBindings.Add(key);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var meme in this.memes)
            {
                this.SetKeyBindings(meme);
            }
        }
    }
}
