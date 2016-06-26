using System;
using System.Threading;
using CoreLocation;
using CoreMotion;
using Foundation;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using UIKit;
using System.Collections.Generic;
using AugmentedRealityCross.World;
using CoreGraphics;

namespace AugmentedRealityCross.iOS
{
	public partial class ViewController : UIViewController
	{
		int count = 1;

		public ViewController (IntPtr handle) : base (handle)
		{
		}

        private MainViewModel ViewModel { get; set; }
        private IGeolocator locator { get; set; }
        private CMMotionManager motion { get; set; }
        public async override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Perform any additional setup after loading the view, typically from a nib.
			Button.AccessibilityIdentifier = "myButton";
			Button.TouchUpInside += delegate {
				var title = string.Format ("{0} clicks!", count++);
				Button.SetTitle (title, UIControlState.Normal);
			};


             locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 50;

            var position = await locator.GetPositionAsync(10000);
            locator.PositionChanged += Locator_PositionChanged;
		    await locator.StartListeningAsync(0, 0, false);
            Console.WriteLine("Position Status: {0}", position.Timestamp);
            Console.WriteLine("Position Latitude: {0}", position.Latitude);
            Console.WriteLine("Position Longitude: {0}", position.Longitude);
            Button.SetTitle($"{position.Latitude} {position.Longitude}", UIControlState.Normal);


            ViewModel = new MainViewModel(WorldConfiguration.iOS);


            ViewModel.UpdateWorld(View.Bounds.Width,View.Bounds.Height);

            //RootLayout = this.FindViewById<RelativeLayout>(Resource.Id.myMainLayout);


            motion = new CMMotionManager();
            motion.StartDeviceMotionUpdates(NSOperationQueue.CurrentQueue, MotionHandler);

            PopulateWorld();
		}

        private IDictionary<UITextView, IWorldElement<Event>> events = new Dictionary<UITextView, IWorldElement<Event>>();

        private void PopulateWorld()
        {
            var elements = ViewModel.WorldEvents;
            foreach (var evt in elements)
            {
                var tv = new UITextView { Text = evt.Element.Type, Bounds = new CGRect(0,0,100,50),
                    TextAlignment = UITextAlignment.Center, TextColor = UIColor.Black, Font=UIFont.PreferredTitle1};
                View.Add(tv);

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

        private int Updating;
        private void MotionHandler(CMDeviceMotion data, NSError error)
        {

            if (Interlocked.CompareExchange(ref Updating, 1, 0) == 1) return;
            InvokeOnMainThread(() =>
            {
                UpdateElementsOnScreen((float)data.Attitude.Roll, (float)data.Attitude.Pitch, (float)data.Attitude.Yaw);
                Interlocked.Exchange(ref Updating, 0);
            });

        }
        private void Locator_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            ViewModel.UpdateWorldLocation(e.Position.Latitude,e.Position.Longitude);

            Button.SetTitle($"{e.Position.Latitude} {e.Position.Longitude}", UIControlState.Normal);
        }

        public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

        private void UpdateElementsOnScreen(float roll, float pitch, float yaw)
        {
            //var roll = reading.RollDegrees * Math.PI / 180.0;
            //var pitch = reading.PitchDegrees * Math.PI / 180.0;
            //var yaw = reading.YawDegrees * Math.PI / 180.0;

            foreach (var evt in events)
            {
                var fe = evt.Key as UITextView;
                if (fe == null || fe.Bounds.Height == 0 || fe.Bounds.Width == 0) continue;
                //var element = fe.DataContext as IWorldElement<Event>;
                //if (element == null) continue;
                var element = events[fe];
                if (element == null) continue;

                var offset = ViewModel.CalculateScreenOffset(element, fe.Bounds.Width, fe.Bounds.Height, roll, pitch, yaw);
                var tf=
                    CGAffineTransform.MakeTranslation((float) offset.TranslateX, (float) offset.TranslateY);
                tf.Scale((float) offset.Scale, (float) offset.Scale);
                fe.Transform = tf;
            }
        }
    }
}

