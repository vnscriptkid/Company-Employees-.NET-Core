using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.JsonPatch;
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

        [HttpGet("{employeeId}", Name = "GetSingleEmployeeOfCompany")]
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

        [HttpPost(Name = "CreateEmployeeForCompany")]
        public IActionResult CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employeeDto)
        {
            if (employeeDto == null)
            {
                _logger.LogError("EmployeeForCreationDto object is missing in request sent from client.");
                return BadRequest("EmployeeForCreationDto object is missing in the body");
            }

            // make sure company exists
            var company = _repo.Company.GetCompany(companyId, trackChanges: false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id {companyId} does not exist in the database.");
                return NotFound();
            }

            // map to employee model
            var employee = _mapper.Map<Employee>(employeeDto);

            // create new employee
            _repo.Employee.CreateEmployee(companyId: companyId, employee: employee);

            // save

            _repo.Save();

            // map to dto
            var employeeToReturn = _mapper.Map<EmployeeDto>(employee);

            // return new employee dto
            return CreatedAtRoute(
                routeName: "GetSingleEmployeeOfCompany", 
                routeValues: new { companyId = companyId, employeeId = employeeToReturn.Id }, 
                employeeToReturn);
        }

        [HttpDelete("{employeeId}", Name = "DeleteEmployeeOfCompany")]
        public IActionResult DeleteEmployeeOfCompany(Guid companyId, Guid employeeId)
        {
            // find company
            var company = _repo.Company.GetCompany(companyId, trackChanges: false);

            if (company == null)
            {
                _logger.LogError($"[DeleteEmployeeOfCompany] Company with id {companyId} does not exist");
                return NotFound();
            }

            // find employee
            var employee = _repo.Employee.GetEmployee(companyId, employeeId, trackChanges: false);
            
            if (employee == null)
            {
                _logger.LogError($"[DeleteEmployeeOfCompany] Employee with id {companyId} and companyId {companyId} does not exist");
                return NotFound();
            }

            // delete employee
            _repo.Employee.DeleteEmployee(employee);

            // save
            _repo.Save();

            // return
            return NoContent();
        }

        [HttpPut("{employeeId}", Name = "UpdateEmployeeForCompany")]
        public IActionResult UpdateEmployeeForCompany(Guid companyId, Guid employeeId, [FromBody]
            EmployeeForUpdateDto employeeUpdateDto)
        {
            if (employeeUpdateDto == null)
            {
                _logger.LogError("EmployeeForUpdateDto object sent from client is null.");
                return BadRequest("EmployeeForUpdateDto object is null");
            }

            var company = _repo.Company.GetCompany(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employee = _repo.Employee.GetEmployee(companyId, employeeId, trackChanges: true);

            if (employee == null)
            {
                _logger.LogInfo($"Employee with id: {employeeId} doesn't exist in the database.");
                return NotFound();
            }

            _mapper.Map(employeeUpdateDto, employee);

            _repo.Save();

            return NoContent();
        }

        [HttpPatch("{employeeId}")]
        public IActionResult PartiallyUpdateEmployeeForCompany(Guid companyId, Guid employeeId,
        [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                _logger.LogError("patchDoc object sent from client is null.");
                return BadRequest("patchDoc object is null");
            }

            var company = _repo.Company.GetCompany(companyId, trackChanges: false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employee = _repo.Employee.GetEmployee(companyId, employeeId, trackChanges: true);

            if (employee == null)
            {
                _logger.LogInfo($"Employee with id: {employeeId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employee);

            patchDoc.ApplyTo(employeeToPatch);

            _mapper.Map(employeeToPatch, employee);

            _repo.Save();

            return NoContent();
        }
    }
}
