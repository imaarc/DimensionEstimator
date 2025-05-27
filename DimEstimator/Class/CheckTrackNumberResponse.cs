using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DimEstimator.Class
{
    public class CheckTrackNumberResponse
    {
        public bool success { get; set; }
        public TrackingNumberExistObj data { get; set; }
        public string message { get; set; }
    }
    public class TrackingNumberExistObj
    {
        public int id { get; set; }
        public string trackingNumber { get; set; }
        public bool isChild { get; set; }
        public bool isExist { get; set; }
    }
    [Serializable]
    public class ScannedItem
    {
        public int Id { get; set; }
        public string TrackingNumber { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
    }


}