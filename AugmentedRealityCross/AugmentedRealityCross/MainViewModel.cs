using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AugmentedRealityCross.World;

namespace AugmentedRealityCross
{
    public class MainViewModel
    {
        #region Events Data

        public Event[] Events { get; } =
            {
            new Event
                {
                    Type = "North",
                   Latitude = -33.863143, Longitude = 151.209900
                },
                new Event
                {
                    Type = "East",
                    Latitude = -33.865143, Longitude = 151.229900
                },
                new Event
                {
                    Type = "South",
                    Latitude = -33.867143, Longitude = 151.209900
                },
                new Event
                {
                    Type = "West",
                    Latitude = -33.865143, Longitude = 151.189900
                }
                ,new Event
                {
                    Type = "Dam",
                    Latitude = -33.839615,
                    Longitude = 151.211325,
                    Description =
                        "Warragamba Dam - There are seven major dams storing Sydney's water. There are also secondary storage dams which hold water, which is available for transfer to the major dams. Water storage dams hold large amounts of water for use over time, particularly in dry conditions. The dams also allow time for many of the 'contaminants' which are held in the water as it runs through the catchments, to settle out. "
                },
                new Event
                {
                    Type = "Dam",
                    Latitude = -34.261757,
                    Longitude = 150.803146,
                    Description =
                        "Cataract Dam - There are seven major dams storing Sydney's water. There are also secondary storage dams which hold water, which is available for transfer to the major dams. Water storage dams hold large amounts of water for use over time, particularly in dry conditions. The dams also allow time for many of the 'contaminants' which are held in the water as it runs through the catchments, to settle out."
                },
                new Event
                {
                    Type = "Dam",
                    Latitude = -33.883527,
                    Longitude = 150.599899,
                    Description =
                        "Wingecarribee Dam - There are seven major dams storing Sydney's water. There are also secondary storage dams which hold water, which is available for transfer to the major dams. Water storage dams hold large amounts of water for use "
                },
                new Event
                {
                    Type = "Catchment",
                    Latitude = -33.811102,
                    Longitude = 151.213331,
                    Description =
                        "When it rains, water can run in from a creek or stream leading to one of the major water supply dams. There are over 16,500 square kilometres of 'catchment area' in Sydney's water system. We sample many of the rivers and streams (inflows) that carry water into the storages from the catchments. "
                },
                new Event
                {
                    Type = "Catchment",
                    Latitude = -33.828216,
                    Longitude = 151.244488,
                    Description =
                        "When it rains, water can run in from a creek or stream leading to one of the major water supply dams. There are over 16,500 square kilometres of 'catchment area' in Sydney's water system. We sample many of the rivers and streams (inflows) that carry water into the storages from the catchments. "
                },
                new Event
                {
                    Type = "Water Filtration",
                    Latitude = -33.874655,
                    Longitude = 151.192732,
                    Description =
                        "On leaving the dams, the water passes to one of our Water Filtration Plants around the Sydney Water network. The filtration plants are designed to further boost the quality by removing identified contaminants which are set down in each plant's performance targets. These are to ensure the water meets quality and health guidelines. "
                },
                new Event
                {
                    Type = "Water Filtration",
                    Latitude = -33.833655,
                    Longitude = 151.261025,
                    Description =
                        "On leaving the dams, the water passes to one of our Water Filtration Plants around the Sydney Water network. The filtration plants are designed to further boost the quality by removing identified contaminants which are set down in each plant's performance targets. These are to ensure the water meets quality and health guidelines. "
                },
                new Event
                {
                    Type = "Desalination",
                    Latitude = -33.813884,
                    Longitude = 151.179857,
                    Description =
                        "Water from the desalination plant at Kurnell will reach up to 1.5 million people as part or all of their water supply. While desalination can provide up to 15% of Sydney's water supply needs, Sydney Water has designed the plant so it can be quickly upgraded to twice its size, or 30% of our water supply needs if necessary."
                },
                new Event
                {
                    Type = "Customer Supply",
                    Latitude = -33.83335,
                    Longitude = 151.154534,
                    Description =
                        "From the Filtration Plant water enters a complex series of pipes and reservoirs for delivery to homes and businesses. Each Filtration Plant may supply one or more customer supply systems. The Prospect Water Filtration Plant, the largest and most complex, supplies water to around 80 per cent of Sydney.  RoadUsers"
                },
                new Event
                {
                    Type = "Office",
                    Latitude = -33.816781,
                    Longitude = 151.005637,
                    Description =
                        "Address: 1 Smith Street, Parramatta, New South Wales. Sydney Water Head Office is an A Grade building comprising 15 levels of office accommodation, ground floor foyer and retail, and basement storage/parking levels accommodating 252 vehicles."
                }
            };

        #endregion

        private ScreenWorld World { get; }

        public MainViewModel(WorldConfiguration conf)
        {
            World = new ScreenWorld(conf);
            foreach (var evt in Events)
            {
                World.AddElementToWorld(evt);
            }
            World.UpdateRangeOfWorld(50.0);
        }

        public void UpdateWorld(double screenWidth, double screenHeight)
        {
            World.Initialize(screenWidth, screenHeight);
        }

        public void UpdateWorldLocation(double latitude, double longitude)
        {
            World.UpdateCentre(new Location {Latitude = latitude,Longitude = longitude});
        }

        public IWorldElement<Event>[] WorldEvents => World.ElementsInWorld<Event>().ToArray();

        public ScreenOffset    CalculateScreenOffset(IWorldElement<Event> element, double width, double height, double roll, double pitch, double yaw)
        {
            //roll = 0;
            //pitch = MathHelper.PiOver2;
            //yaw = 0;// MathHelper.PiOver2;
            return World.Offset(element, new Rectangle(0, 0, (int) width, (int) height), roll,pitch,yaw);
        }
    }

    public class Event:IHasLocation
    {
        public string Type { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description { get; set; }

        public Location Location => new Location { Latitude = Latitude, Longitude = Longitude };

        public Location GeoLocation => Location;
    }
}
