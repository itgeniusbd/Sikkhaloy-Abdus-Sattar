using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AttendanceDevice
{
    /// <summary>
    /// Interaction logic for TESTWindow.xaml
    /// </summary>
    public partial class TESTWindow : Window
    {
        private DispatcherTimer _tmr = new DispatcherTimer();
        private List<BitmapImage> _lstImages = new List<BitmapImage>();
        private int _intCurrentImageIndex = 0;

        public TESTWindow()
        {
            InitializeComponent();

            _lstImages.Add(new BitmapImage(new Uri("/images/image1.png", UriKind.RelativeOrAbsolute)));
            _lstImages.Add(new BitmapImage(new Uri("/images/image2.png", UriKind.RelativeOrAbsolute)));
            _lstImages.Add(new BitmapImage(new Uri("/images/image3.png", UriKind.RelativeOrAbsolute)));


            //Initialize the first Image-control (i.e. display the very first image)
            //img1.Source = _lstImages[_intCurrentImageIndex];

            //Timer-setup
            _tmr.Interval = new TimeSpan(0, 0, 3);
            _tmr.Tick += new EventHandler(Timer_Tick);
            _tmr.Start();
            this.Closing += new System.ComponentModel.CancelEventHandler(Window_Closing);
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Clean up.
            _tmr.Stop();
            _tmr = null;
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            //Either get the next image's index or start over with the first image.
            if (_intCurrentImageIndex + 1 >= _lstImages.Count)
                _intCurrentImageIndex = 0;
            else
                _intCurrentImageIndex++;

            //Set the source of the image to fade in ...
            img1.Source = _lstImages[_intCurrentImageIndex];

            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 1;
            da.Duration = new Duration(TimeSpan.FromMilliseconds(1200));
            da.AutoReverse = true;
            img1.BeginAnimation(OpacityProperty, da);
        }
    }
}

