﻿using Contracts;
using Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository
{
    public class RepositoryManager : IRepositoryManager
    {
        private readonly RepositoryContext _repositoryContext;
        private IEmployeeRepository _employeeRepository;
        private ICompanyRepository _companyRepository;

        public RepositoryManager(RepositoryContext repositoryContext)
        {
            _repositoryContext = repositoryContext;
        }

        public ICompanyRepository Company
        {
            get
            {
                if (_companyRepository == null)
                {
                    _companyRepository = new CompanyRepository(_repositoryContext);
                }
                return _companyRepository;
            }
        }

        public IEmployeeRepository Employee
        {
            get
            {
                if (_employeeRepository == null)
                {
                    _employeeRepository = new EmployeeRepository(_repositoryContext);
                }
                return _employeeRepository;
            }
        }

        public Task SaveAsync() => _repositoryContext.SaveChangesAsync();
    }
}
