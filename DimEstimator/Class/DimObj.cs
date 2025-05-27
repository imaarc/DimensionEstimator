using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DimEstimator.Class
{
    public class DimObj
    {
        public string DimensionsEstimate { get; set; }
    }
    public class Estimate
    {
        public double length { get; set; }
        public double width { get; set; }
        public double height { get; set; }
    }
}