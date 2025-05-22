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
        public int length { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
}