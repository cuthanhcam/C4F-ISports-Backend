using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Field.AddressValidationDtos;

namespace api.Interfaces
{
    public interface IGeocodingService
    {
        Task<AddressValidationResultDto> ValidateAddressAsync(ValidateAddressDto addressDto);
    }
}   