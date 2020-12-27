using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Entities.DataTransferObjects
{
    public class EmployeeForCreationDto
    {
        [Required(ErrorMessage = "Employee name is a required field.")]
        [MaxLength(30, ErrorMessage = "Maximum length for the Name is 30 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Age is a required field.")]
        [Range(minimum:18, maximum:80, ErrorMessage = "Age is required and must be from 18 to 80.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Position is a required field.")]
        [MaxLength(20, ErrorMessage = "Maximum length for the Position is 20 characters.")]
        public string Position { get; set; }
    }
}
