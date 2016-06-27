using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Hardware;
using Android.Locations;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using AugmentedRealityCross.World;
using Location = Android.Locations.Location;

namespace AugmentedRealityCross.Droid
{
    [Activity(Label = "AugmentedRealityCross.Droid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, ILocationListener, ISensorEventListener
    {
        public MainViewModel ViewModel { get; set; }

        private LocationManager locationManager;

        private SensorManager sensorManager;
        private Sensor orientation;

        private RelativeLayout RootLayout { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            ViewModel = new MainViewModel(WorldConfiguration.Android);

            var metrics = Resources.DisplayMetrics;
            var widthInDp = ConvertPixelsToDp(metrics.WidthPixels);
            var heightInDp = ConvertPixelsToDp(metrics.HeightPixels);

            ViewModel.UpdateWorld(metrics.WidthPixels, metrics.HeightPixels);

            RootLayout = this.FindViewById<RelativeLayout>(Resource.Id.myMainLayout);

            locationManager = GetSystemService(Context.LocationService) as LocationManager;
            locationManager.RequestLocationUpdates(LocationManager.NetworkProvider, 50, 0, this);

            sensorManager = GetSystemService(Context.SensorService) as SensorManager;
            //orientation= sensorManager?.GetDefaultSensor(SensorType.Orientation);
            //sensorManager.RegisterListener(this,orientation, SensorDelay.Normal);

            accelerometer = sensorManager?.GetDefaultSensor(SensorType.Accelerometer);
            magnetometer = sensorManager?.GetDefaultSensor(SensorType.MagneticField);
            sensorManager.RegisterListener(this, accelerometer, SensorDelay.Ui);
            sensorManager.RegisterListener(this, magnetometer, SensorDelay.Ui);


            PopulateWorld();
        }

        Sensor accelerometer;
        Sensor magnetometer;

        //protected void onPause()
        //{
        //    super.onPause();
        //    mSensorManager.unregisterListener(this);
        //}

        public void onAccuracyChanged(Sensor sensor, int accuracy) { }

        float[] mGravity;
        float[] mGeomagnetic;
        public void OnSensorChanged(SensorEvent evt)
        {
            if (evt.Sensor.Type == SensorType.Accelerometer)
                mGravity = evt.Values.ToArray();
            if (evt.Sensor.Type == SensorType.MagneticField)
                mGeomagnetic = evt.Values.ToArray();
            if (mGravity != null && mGeomagnetic != null)
            {
                var R = new float[9];
                var I = new float[9];
                var success = SensorManager.GetRotationMatrix(R, I, mGravity, mGeomagnetic);
                if (success)
                {
                    var orientation = new float[3];
                    SensorManager.GetOrientation(R, orientation);

                    if (Interlocked.CompareExchange(ref Updating, 1, 0) == 1) return;
                    RunOnUiThread(() =>
                    {
                        UpdateElementsOnScreen(orientation[2], orientation[1], orientation[0]);
                        Interlocked.Exchange(ref Updating, 0);
                    });
                }
            }
        }


        private int ConvertPixelsToDp(float pixelValue)
        {
            var dp = (int)((pixelValue) / Resources.DisplayMetrics.Density);
            return dp;
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            ViewModel.UpdateWorld(newConfig.ScreenWidthDp, newConfig.ScreenHeightDp);
        }

        public void OnLocationChanged(Location location)
        {
            ViewModel.UpdateWorldLocation(location.Latitude, location.Longitude);
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }


        private int Updating;
        public void OnSensorChangedOld(SensorEvent e)
        {

            if (Interlocked.CompareExchange(ref Updating, 1, 0) == 1) return;
            RunOnUiThread(() =>
            {
                UpdateElementsOnScreen(e.Values[2], e.Values[1], e.Values[0]);
                Interlocked.Exchange(ref Updating, 0);
            });

        }

        private IDictionary<TextView, IWorldElement<Event>> events = new Dictionary<TextView, IWorldElement<Event>>();

        private void PopulateWorld()
        {
            var elements = ViewModel.WorldEvents;
            foreach (var evt in elements)
            {
                var tv = new TextView(this) { Text = evt.Element.Type };
                RootLayout.AddView(tv);

                events[tv] = evt;
                //        < Button
                //android: id = "@+id/ok"
                //android: layout_width = "wrap_content"
                //android: layout_height = "wrap_content"
                //android: layout_below = "@id/entry"
                //android: layout_alignParentRight = "true"
                //android: layout_marginLeft = "10dip"
                //android: text = "OK" />

                //         var tb = new TextBlock { Text = evt.Element.Type, DataContext = evt };
                //        LayoutRoot.Children.Add(tb);
            }

        }

        private void UpdateElementsOnScreen(float roll, float pitch, float yaw)
        {
            //roll = (float)((double)roll).ToRad();
            //pitch = (float)((double)pitch).ToRad();
            //yaw = (float)((double)yaw).ToRad();

            //var roll = reading.RollDegrees * Math.PI / 180.0;
            //var pitch = reading.PitchDegrees * Math.PI / 180.0;
            //var yaw = reading.YawDegrees * Math.PI / 180.0;
            var cnt = RootLayout.ChildCount;
            for (int i = 0; i < cnt; i++)
            {
                var fe = RootLayout.GetChildAt(i) as TextView;
                if (fe == null || fe.Height == 0 || fe.Width == 0)
                    continue;
                //var element = fe.DataContext as IWorldElement<Event>;
                //if (element == null) continue;
                var element = events[fe];
                if (element == null)
                    continue;


                var offset = ViewModel.CalculateScreenOffset(element, fe.Width, fe.Height, roll, pitch, yaw);
                fe.TranslationX = (float)offset.TranslateX;
                fe.TranslationY = (float)offset.TranslateY;
                fe.ScaleX = (float)offset.Scale;
                fe.ScaleY = (float)offset.Scale;

                //System.Diagnostics.Debug.WriteLine($"{element.Element.Type} {offset.TranslateX} {offset.TranslateY} {offset.Scale}");

                //                RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(30, 40);
                //params.leftMargin = 50;
                //params.topMargin = 60;
                //                rl.addView(iv, params);

                //var offset = ViewModel.CalculateScreenOffset(element, fe.ActualWidth, fe.ActualHeight, roll, pitch, yaw);
                //fe.RenderTransform = new CompositeTransform
                //{
                //    TranslateX = offset.TranslateX,
                //    TranslateY = offset.TranslateY,
                //    ScaleX = offset.Scale,
                //    ScaleY = offset.Scale
                //};
            }
        }
    }
}


