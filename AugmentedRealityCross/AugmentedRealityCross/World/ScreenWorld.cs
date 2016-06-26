using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace AugmentedRealityCross.World
{
    public enum WorldConfiguration
    {
        Android,
        iOS,
        WindowsMobile
    }
    public class ScreenWorld
    {
        public static IDictionary<WorldConfiguration, Tuple<Vector3,Matrix>> Configurations =
        new Dictionary<WorldConfiguration, Tuple<Vector3, Matrix>>
        {
          {WorldConfiguration.Android, new Tuple<Vector3,Matrix>(Vector3.Up, Matrix.CreateRotationY(MathHelper.PiOver2) *Matrix.CreateRotationZ(-MathHelper.PiOver2))},
          {WorldConfiguration.iOS, new Tuple<Vector3,Matrix>(Vector3.Down, Matrix.CreateRotationZ(-MathHelper.PiOver2))},
          {WorldConfiguration.WindowsMobile, new Tuple<Vector3,Matrix>(Vector3.Down, Matrix.CreateRotationY(MathHelper.PiOver2) *Matrix.CreateRotationZ(-MathHelper.PiOver2))}
        };


    private Viewport Viewport { get; set; }
        private Matrix Projection { get; set; }
        private Matrix View { get; set; }

        public double VisualRangeInKilometres { get; private set; } = 50.0;

        public Location CentreOfWorld { get; private set; } = new Location { Latitude = -33.865143, Longitude = 151.209900 };

        private IList<IGenericWorldElement> WorldElements { get; } = new List<IGenericWorldElement>();

        private WorldConfiguration Configuration { get; }

        private Vector3 CameraUp{ get; }
        private Matrix WorldAdjustment { get; }

        public ScreenWorld(WorldConfiguration config)
        {
            Configuration = config;
            var conf = Configurations[config];
            CameraUp = conf.Item1;
            WorldAdjustment = conf.Item2;
            Initialize();
        }

        public IEnumerable<IWorldElement<  TElement>> ElementsInWorld<TElement>()  where TElement:IHasLocation
        {
            return WorldElements.OfType<IWorldElement<TElement>>().ToArray();
        }

        public bool Initialize(double screenWidth=500, double screenHeight=500)
        {
            if (screenWidth <= 0 || screenHeight <= 0) return false;

            // Initialize the viewport and matrixes for 3d projection.
            Viewport = new Viewport(0, 0, (int) screenWidth, (int) screenHeight);
            float aspect = Viewport.AspectRatio;
            Projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 100);
            View = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, CameraUp );
            return true;
        }

        public ScreenOffset Offset<TElement> (IWorldElement<TElement> point, Rectangle bounds, double roll,double pitch, double yaw) where TElement:IHasLocation
        {
            #region HACK based on bearing
            //var offset = new ScreenOffset();
            //var bearing = CentreOfWorld.Bearing(point.Element.GeoLocation);
            //var diff = yaw - bearing;

            //// let vof be 90 degrees (45 each side of straight ahead
            //var vof = (90.0).ToRad();
            //offset.TranslateX = (Viewport.Width/2)*(1 + (diff/vof)); 
            //offset.TranslateY = Viewport.Height/2;

            //var distance = CentreOfWorld.DistanceInKilometres(point.Element.GeoLocation);
            //offset.Scale = 1; // Do something here with distance and range etc
            //return offset;
            #endregion HACK

            var mat = Matrix.CreateFromYawPitchRoll((float)yaw, (float)pitch, (float)roll);
            var currentAttitude = WorldAdjustment *
                mat;

            //var currentAttitude =                                     // Required for Android
            //   Matrix.CreateRotationY(MathHelper.PiOver2) *
            //   Matrix.CreateRotationZ(-MathHelper.PiOver2) *
            //   mat;

            //var currentAttitude =                                     // Required for iOS
            //  Matrix.CreateRotationZ(-MathHelper.PiOver2) *
            //  mat;

            //        var currentAttitude =                             // REquired for UWP Mobile
            //Matrix.CreateRotationY(MathHelper.PiOver2) *
            //Matrix.CreateRotationZ(-MathHelper.PiOver2) *
            //mat;


            return WorldHelpers.Offset(point.PositionInWorld, bounds, Viewport, Projection, View, currentAttitude);
        }

        public void UpdateRangeOfWorld(double rangeInKilometres)
        {
            VisualRangeInKilometres = rangeInKilometres;
            RepositionElements();
        }

        public void UpdateCentre(Location newCentreOfWorld)
        {
            // TODO: allow centre to update
            return;
            CentreOfWorld = newCentreOfWorld;
            RepositionElements();
        }

        public void  AddElementToWorld<TElement>(TElement element) where TElement: IHasLocation
        {
            var wrapper = new ElementWrapper<TElement> {Element = element};
            WorldElements.Add(wrapper);
            wrapper.PositionInWorld = DeterminePositionInWorld(element);
            //wrapper.PositionInWorld;
        }

        private void RepositionElements()
        {
            foreach (var worldElement in WorldElements)
            {
                worldElement.PositionInWorld = DeterminePositionInWorld(worldElement.ElementWithLocation);
            }
        }

        private Vector3 DeterminePositionInWorld(IHasLocation element)
        {

            var eventItem = element.GeoLocation;
            var eastWestLocation = new Location {Latitude = CentreOfWorld.Latitude, Longitude = eventItem.Longitude};
            var eastWestDistance = eastWestLocation.DistanceInMetres(CentreOfWorld);
            if (eastWestLocation.Longitude < CentreOfWorld.Longitude)
            {
                eastWestDistance *= -1;
            }

            var northSouthDistance = eventItem.DistanceInMetres(eastWestLocation);
            if (eventItem.Latitude > eastWestLocation.Latitude)
            {
                northSouthDistance *= -1;
            }

            // Make sure there's a valid range
            if (VisualRangeInKilometres <= 0)
            {
                VisualRangeInKilometres =1.0;
            }

            // AddDirectionPoints((int)(-eastWestDistance/ 1000.0), 0, (int)(-northSouthDistance / 1000.0), eventItem.Type);
            return new Vector3((float) (eastWestDistance /(VisualRangeInKilometres)), 0, (float) (northSouthDistance/ (VisualRangeInKilometres )));

        }

        private interface IGenericWorldElement
        {
            IHasLocation ElementWithLocation { get; }
            Vector3 PositionInWorld { get; set; }
        }

        private class ElementWrapper<TElement>: IGenericWorldElement, IWorldElement<TElement> where TElement:IHasLocation
        {
            public TElement Element { get; set; }
            public IHasLocation ElementWithLocation => Element;
            public Vector3 PositionInWorld { get; set; }

        }
    }

    public interface IHasLocation
    {
        Location GeoLocation { get;  }

    }

    public interface IWorldElement<TElement> where TElement:IHasLocation
    {
        TElement Element { get; }
        Vector3 PositionInWorld { get; }
    }
}
