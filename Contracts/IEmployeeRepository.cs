﻿using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts
{
    public interface IEmployeeRepository
    {
        IEnumerable<Employee> GetEmployees(Guid companyId, bool trackChanges);
        Employee GetEmployee(Guid companyId, Guid employeeId, bool trackChanges);
        void CreateEmployee(Guid companyId, Employee employee);
        void DeleteEmployee(Employee employee);
    }
}
