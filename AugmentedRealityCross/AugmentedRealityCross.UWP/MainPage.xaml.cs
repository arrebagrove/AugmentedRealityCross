using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using AugmentedRealityCross.World;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AugmentedRealityCross.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MainViewModel ViewModel => DataContext as MainViewModel;

        private Inclinometer inclinometer;
        private Geolocator geolocator;


        public MainPage()
        {
            this.InitializeComponent();

            DataContext = new MainViewModel(WorldConfiguration.WindowsMobile);

            SizeChanged += MainPage_SizeChanged;
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Update dimensions of the world
            ViewModel.UpdateWorld(this.ActualWidth, this.ActualHeight);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Update dimensions of the world
            ViewModel.UpdateWorld(this.ActualWidth, this.ActualHeight);

            // Get location and update centre of world
            var accessStatus = await Geolocator.RequestAccessAsync();
            geolocator = new Geolocator();
            var position = await geolocator.GetGeopositionAsync();
            UpdateLocation(position);
            geolocator.PositionChanged += Geolocator_PositionChanged;


            inclinometer = Inclinometer.GetDefault();
            var inclination = inclinometer.GetCurrentReading();
            PopulateWorld();

            inclinometer.ReadingChanged += Inclinometer_ReadingChanged;
            inclinometer.ReportInterval = 1;
            UpdateElementsOnScreen(inclination);
        }

        private void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateLocation(args.Position);
            });
        }
        private void UpdateLocation(Geoposition position)
        {

            ViewModel.UpdateWorldLocation(position.Coordinate.Point.Position.Latitude, position.Coordinate.Point.Position.Longitude);
        }

        private void PopulateWorld()
        {
            var elements = ViewModel.WorldEvents;
            foreach (var evt in elements)
            {
                var tb = new TextBlock { Text = evt.Element.Type, DataContext = evt };
                LayoutRoot.Children.Add(tb);
            }

        }


        private int Updating;
        private void Inclinometer_ReadingChanged(Windows.Devices.Sensors.Inclinometer sender, Windows.Devices.Sensors.InclinometerReadingChangedEventArgs args)
        {
            if (Interlocked.CompareExchange(ref Updating, 1, 0) == 1) return;
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateElementsOnScreen(args.Reading);
                Interlocked.Exchange(ref Updating, 0);
            });

        }

        private void UpdateElementsOnScreen(InclinometerReading reading)
        {
            var roll = reading.RollDegrees * Math.PI / 180.0;
            var pitch = reading.PitchDegrees * Math.PI / 180.0;
            var yaw = reading.YawDegrees * Math.PI / 180.0;

            foreach (var child in LayoutRoot.Children)
            {
                var fe = (child as FrameworkElement);
                if (fe == null || fe.ActualHeight==0 || fe.ActualWidth==0) continue;
                var element = fe.DataContext as IWorldElement<Event>;
                if (element == null) continue;

                var offset = ViewModel.CalculateScreenOffset(element, fe.ActualWidth, fe.ActualHeight, roll, pitch, yaw);
                if (offset.TranslateX < -this.ActualWidth)
                {
                    offset.TranslateX = -this.ActualWidth;
                }
                if (offset.TranslateX > this.ActualWidth*2)
                {
                    offset.TranslateX = this.ActualWidth*2;
                }
                if (offset.TranslateY < -this.ActualHeight)
                {                   
                    offset.TranslateY = -this.ActualHeight;
                }                   
                if (offset.TranslateY > this.ActualHeight * 2)
                {                   
                    offset.TranslateY = this.ActualHeight* 2;
                }
                if (offset.Scale < 0)
                {
                    offset.Scale = 0.0001;
                }
                if (offset.Scale > 2)
                {
                    offset.Scale = 2;
                }
                fe.RenderTransform = new CompositeTransform
                {
                    TranslateX = offset.TranslateX,
                    TranslateY = offset.TranslateY,
                    ScaleX = offset.Scale,
                    ScaleY = offset.Scale
                };
            }
        }

    }
}
