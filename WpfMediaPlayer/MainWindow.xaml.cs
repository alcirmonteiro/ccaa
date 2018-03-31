using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace WpfMediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public Timer TempoExecucao { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            IsPlaying(false);
            imgSnapShot.Visibility = Visibility.Hidden;
        }

        private void IsPlaying(bool flag)
        {
            btnPlay.IsEnabled = flag;
            btnStop.IsEnabled = flag;
            btnMoveBack.IsEnabled = flag;
            btnMoveForward.IsEnabled = flag;
        }

        #region Eventos dos Botões de Navegação

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            IsPlaying(true);
            if (btnPlay.Content.ToString() == "Play")
            {
                MediaPlayer.Play();
                btnPlay.Content = "Pause";
                TempoExecucao.Start();
            }
            else
            {
                MediaPlayer.Pause();
                btnPlay.Content = "Play";
                TempoExecucao.Stop();
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Pause();
            btnPlay.Content = "Play";
            IsPlaying(false);
            btnPlay.IsEnabled = true;
            TempoExecucao.Stop();
        }

        private void btnMoveBack_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position -= TimeSpan.FromSeconds(1);
            AtualizarValorSlider(-1);
        }

        private void btnMoveForward_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position += TimeSpan.FromSeconds(1);
            AtualizarValorSlider(1);
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box 
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Videos"; // Default file name 
            dialog.DefaultExt = ".MP4"; // Default file extension 
            dialog.Filter = "MP4 file (.mp4)|*.mp4"; // Filter files by extension  

            // Show open file dialog box 
            Nullable<bool> result = dialog.ShowDialog();

            // Process open file dialog box results  
            if (result == true)
            {
                // Open document  
                MediaPlayer.Source = new Uri(dialog.FileName);
                btnPlay.IsEnabled = true;
                
                slValue.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler(Comandos_DragStarted));
                slValue.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler‌(Comandos_DragCompleted));
                slValue.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(Comandos_DragDelta));

                TempoExecucao = new Timer(1000);
                TempoExecucao.Elapsed += TempoExecucao_Elapsed;
            }


        }

        private void SnapShot()
        {
            Size dpi = new Size(96, 96);
            RenderTargetBitmap bmp =
              new RenderTargetBitmap(300, 200,
                dpi.Width, dpi.Height, PixelFormats.Pbgra32);
            bmp.Render(MediaPlayer);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            //MemoryStream ms = new MemoryStream();
            //encoder.Save(ms);

            string filename = Guid.NewGuid().ToString() + ".jpg";
            FileStream fs = new FileStream(filename, FileMode.Create);
            encoder.Save(fs);
            fs.Close();
            String path = System.AppDomain.CurrentDomain.BaseDirectory.ToString();
            imgSnapShot.Source = new BitmapImage(new Uri(path + filename));
        }


        #endregion 

        //public void PreviewTextInput(object sender, TextCompositionEventArgs e)
        //{
        //    e.Handled = !IsTextAllowed(e.Text);
        //}

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }


        private void Midia_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.NaturalDuration.HasTimeSpan)
            {
                slValue.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            }
        }

        #region Comandos de Drag
        private void Comandos_DragStarted(object sender, DragStartedEventArgs e)
        {
            TempoExecucao.Stop();
            MediaPlayer.Pause();
            imgSnapShot.Visibility = Visibility.Visible;
        }


        private void Comandos_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            //var posicao = (Slider)sender;
            //MediaPlayer.Position = TimeSpan.FromSeconds(posicao.Value);
            TempoExecucao.Start();
            MediaPlayer.Play();
            imgSnapShot.Visibility = Visibility.Hidden;
        }



        private void Comandos_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var posicao = (Slider)sender;
            MediaPlayer.Position = TimeSpan.FromSeconds(posicao.Value);
            SnapShot();
        }
        #endregion

        public delegate void AtualizarSlider();

        private void AtualizarValorSlider()
        {
            slValue.Value += 1;
        }

        private void AtualizarValorSlider(int valor)
        {
            slValue.Value += valor;
        }

        private void TempoExecucao_Elapsed(Object source, ElapsedEventArgs e)
        {
            slValue.Dispatcher.Invoke(new AtualizarSlider(this.AtualizarValorSlider));
        }


        private void TelaPlayer_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    MediaPlayer.Pause();
                    TempoExecucao.Stop();
                    btnMoveBack_Click(btnMoveBack, new RoutedEventArgs());
                    break;
                case Key.Right:
                    MediaPlayer.Pause();
                    TempoExecucao.Stop();
                    btnMoveForward_Click(btnMoveForward, new RoutedEventArgs());
                    break;
            }
        }

        private void TelaPlayer_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    TempoExecucao.Start();
                    MediaPlayer.Play();
                    break;
                case Key.Right:
                    TempoExecucao.Start();
                    MediaPlayer.Play();
                    break;
            }
        }
    }

}