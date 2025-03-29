using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Interfaces
{
    public interface IGeocodingService
    {
        Task<(decimal latitude, decimal longitude)> GetCoordinatesFromAddressAsync(string address);
    }
}