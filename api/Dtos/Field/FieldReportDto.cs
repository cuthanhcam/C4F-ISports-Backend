using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Field
{
    public class FieldReportDto
    {
        [Required(ErrorMessage = "Lý do báo cáo không được để trống")]
        [StringLength(50, ErrorMessage = "Lý do báo cáo không được vượt quá 50 ký tự")]
        public string Reason { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string Description { get; set; }
    }
} 