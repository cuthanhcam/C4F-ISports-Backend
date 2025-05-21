using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.Field;
using api.Dtos.Field.SubFieldDtos;

namespace api.Interfaces
{
    public interface ISubFieldService
    {
        Task<SubFieldDto> CreateSubFieldAsync(ClaimsPrincipal user, int fieldId, CreateSubFieldDto createSubFieldDto);
        Task<SubFieldDto> UpdateSubFieldAsync(ClaimsPrincipal user, int fieldId, int subFieldId, UpdateSubFieldDto updateSubFieldDto);
        Task DeleteSubFieldAsync(ClaimsPrincipal user, int fieldId, int subFieldId);
    }
}