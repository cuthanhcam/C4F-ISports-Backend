using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Field;

namespace api.Interfaces
{
    public interface IGeocodingService
    {
        Task<ValidateAddressResponseDto> ValidateAddressAsync(ValidateAddressDto addressDto);
    }
}   