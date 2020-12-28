using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Repository.Extensions
{
    public static class RepositoryEmployeeExtensions
    {
        public static IQueryable<Employee> SearchByName(this IQueryable<Employee> employees, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return employees;

            var normalizedSearchTerm = searchTerm.Trim().ToLower();

            return employees.Where(e => e.Name.ToLower().Contains(normalizedSearchTerm));
        }

        public static IQueryable<Employee> FilterByAge(this IQueryable<Employee> employees, uint minAge, uint maxAge) => 
            employees.Where(e => minAge <= e.Age && maxAge >= e.Age);
    }
}
