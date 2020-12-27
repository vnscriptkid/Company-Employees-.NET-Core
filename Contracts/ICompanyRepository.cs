﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Entities.DataTransferObjects;
using Entities.Models;

namespace Contracts
{
    public interface ICompanyRepository
    {
        IEnumerable<Company> GetAllCompanies(bool trackChanges);
        Company GetCompany(Guid companyId, bool trackChanges);
        IEnumerable<Company> GetByIds(IEnumerable<Guid> ids, bool trackChanges);
        void Create(Company company);
        void Delete(Company company);
    }
}
