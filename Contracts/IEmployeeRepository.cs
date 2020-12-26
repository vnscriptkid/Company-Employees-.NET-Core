using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts
{
    public interface IEmployeeRepository
    {
        IEnumerable<Employee> getEmployeesForCompany(Guid companyId, bool trackChanges);
    }
}
