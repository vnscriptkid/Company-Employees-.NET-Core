﻿using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesController : Controller
    {
        private readonly IRepositoryManager _repo;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public EmployeesController(IRepositoryManager repo, ILoggerManager logger, IMapper mapper)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAllEmployeesOfCompany(Guid companyId)
        {
            var company = _repo.Company.GetCompany(companyId, trackChanges: false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id {companyId} does not exist.");
                return NotFound();
            }

            var employees = _repo.Employee.GetEmployees(companyId, trackChanges: false);

            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employees).ToList();

            return Ok(employeesDto);
        }

        [HttpGet("{employeeId}")]
        public IActionResult GetSingleEmployeeOfCompany(Guid companyId, Guid employeeId)
        {
            var company = _repo.Company.GetCompany(companyId, trackChanges: false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id {companyId} does not exist in the database.");
                return NotFound();
            }

            var employee = _repo.Employee.GetEmployee(companyId, employeeId, trackChanges: false);


            if (employee == null)
            {
                _logger.LogInfo($"Employee with id {employeeId} does not exist in the database");
                return NotFound();
            }

            var employeeDto = _mapper.Map<EmployeeDto>(employee); 

            return Ok(employeeDto);
        }
    }
}