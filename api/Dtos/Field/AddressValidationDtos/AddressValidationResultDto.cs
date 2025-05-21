using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Field.AddressValidationDtos
{
    public class AddressValidationResultDto
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string FormattedAddress { get; set; } = string.Empty;
        public bool IsValid { get; set; }
    }
}