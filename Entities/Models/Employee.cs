using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Entities.Models
{
    class Employee
    {
        [Column("EmployeeId")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Employee name is a required field.")]
        [MaxLength(ErrorMessage = "Maximum length for employee name is 30 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Employee age is a required field.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Employee position is a required field.")]
        [MaxLength(ErrorMessage = "Maximum length for employee position is 20 characters.")]
        public string Position { get; set; }

        [ForeignKey(nameof(Company))]
        public Guid CompanyId { get; set; }

        public Company Company { get; set; }
    }
}
