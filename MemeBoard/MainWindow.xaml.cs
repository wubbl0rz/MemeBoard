using mrousavy;
using System;
using System.Collections.Generic;
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
        private Memes currentMeme = Memes.MonkaS;

        public MainWindow()
        {
            InitializeComponent();
            this.Topmost = true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
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

        private void ChangeMeme(Memes meme)
        {
            if (this.IsVisible && this.currentMeme == meme)
            {
                this.Hide();
                return;
            }

            sb.Stop();

            switch (meme)
            {
                case Memes.LUL:
                    ImageBehavior.SetAnimatedSource(image, null);
                    this.image.Source = new BitmapImage(new Uri(@"C:\Users\stream\Desktop\memes\LUL.png"));
                    break;
                case Memes.MonkaS:
                    this.image.Source = new BitmapImage(new Uri(@"C:\Users\stream\Desktop\memes\monkaS.png"));
                    break;
                case Memes.OMEGALUL:
                    this.image.Source = new BitmapImage(new Uri(@"C:\Users\stream\Desktop\memes\omegalul.png"));
                    break;
                case Memes.Confused:
                    this.image.Source = new BitmapImage(new Uri(@"C:\Users\stream\Desktop\memes\confused.png"));
                    break;
                case Memes.LUL3D:
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(@"C:\Users\stream\Desktop\memes\lul3d.gif");
                    img.EndInit();
                    ImageBehavior.SetAnimatedSource(image, img);
                    break;
                default:
                    break;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new HotKey(ModifierKeys.Control, Key.L, this,
                _ => this.ChangeMeme(Memes.LUL));

            new HotKey(ModifierKeys.Control, Key.M, this,
                _ => this.ChangeMeme(Memes.MonkaS));

            new HotKey(ModifierKeys.Control, Key.O, this,
                _ => this.ChangeMeme(Memes.OMEGALUL));

            new HotKey(ModifierKeys.Control, Key.D3, this,
                _ => this.ChangeMeme(Memes.LUL3D));

            new HotKey(ModifierKeys.Control, Key.W, this,
                _ => this.ChangeMeme(Memes.Confused));
        }
    }

    public enum Memes
    {
        LUL,
        LUL3D,
        MonkaS,
        Confused,
        PogChamp,
        POGGERS,
        OMEGALUL,
        ResidentSleeper,
        gachiBASS,
        WutFace,
        cmonBruh
    }
}
