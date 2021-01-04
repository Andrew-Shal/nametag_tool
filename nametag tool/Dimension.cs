using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nametag_tool
{
    public class Dimension
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public Dimension(double width, double height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// return a double value(x > 0, x <= 1 )
        /// optimal_width = (100 / (img_width / container_width)) / 100
        /// optimal_height = (100 / (img_height / container_height)) / 100
        /// </summary>
        /// <param name="parentElem"></param>
        /// <returns>double</returns>
        public static double calculateOptimalZoom(Dimension childElem, Dimension parentElem)
        {
            // only if height or width of image is larger than its container
            if (childElem.Height > parentElem.Height || childElem.Width > parentElem.Width)
            {
                // do both calculations and test which is the smaller
                var eq1 = (100 / (childElem.Width / parentElem.Width)) / 100;
                var eq2 = (100 / (childElem.Height / parentElem.Height)) / 100;

                return eq1 <= eq2 ? eq1 : eq2;
            }
            // image is smaller than it's parent container, return full size 100%
            return 1d;
        }
    }
}
