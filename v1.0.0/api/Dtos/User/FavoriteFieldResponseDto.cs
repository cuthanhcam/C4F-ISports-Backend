using System;

namespace api.Dtos.User
{
    public class FavoriteFieldResponseDto
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public string SportType { get; set; }
        public string Location { get; set; }
        public string Phone { get; set; }
        public string OpenHours { get; set; }
        public string AddedDate { get; set; }
    }
} 