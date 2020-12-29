using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Linq.Dynamic.Core;
using Repository.Extensions.Utility;

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

        public static IQueryable<Employee> Sort(this IQueryable<Employee> employees, string orderByQueryString)
        {
            if (string.IsNullOrWhiteSpace(orderByQueryString))
                return employees.OrderBy(e => e.Name);

            var orderQuery = employees.CreateQueryBuilder(orderByQueryString);

            if (string.IsNullOrWhiteSpace(orderQuery))
                return employees.OrderBy(e => e.Name);

            return employees.OrderBy(orderQuery);
        }
    }
}
