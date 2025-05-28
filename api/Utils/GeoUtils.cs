using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Utils
{
    public static class GeoUtils
    {
        /// <summary>
        /// Chuyển đổi góc từ độ sang radian.
        /// </summary>
        /// <param name="degrees">Góc tính bằng độ.</param>
        /// <returns>Góc tính bằng radian.</returns>
        public static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        /// <summary>
        /// Tính khoảng cách giữa hai điểm trên Trái Đất bằng công thức Haversine (đơn vị: km).
        /// </summary>
        /// <param name="lat1">Vĩ độ điểm 1.</param>
        /// <param name="lon1">Kinh độ điểm 1.</param>
        /// <param name="lat2">Vĩ độ điểm 2.</param>
        /// <param name="lon2">Kinh độ điểm 2.</param>
        /// <returns>Khoảng cách (km).</returns>
        public static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Bán kính Trái Đất (km)
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}