using AutoMapper;
using CompanyEmployees.ActionFilters;
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

        [HttpGet(Name = "GetAllEmployeesOfCompany")]
        public async Task<IActionResult> GetAllEmployeesOfCompany(Guid companyId)
        {
            var company = await _repo.Company.GetCompanyAsync(companyId, trackChanges: false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id {companyId} does not exist.");
                return NotFound();
            }

            var employees = await _repo.Employee.GetEmployeesAsync(companyId, trackChanges: false);

            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employees).ToList();

            return Ok(employeesDto);
        }

        [HttpGet("{employeeId}", Name = "GetSingleEmployeeOfCompany")]
        public async Task<IActionResult> GetSingleEmployeeOfCompany(Guid companyId, Guid employeeId)
        {
            var company = await _repo.Company.GetCompanyAsync(companyId, trackChanges: false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id {companyId} does not exist in the database.");
                return NotFound();
            }

            var employee = await _repo.Employee.GetEmployeeAsync(companyId, employeeId, trackChanges: false);


            if (employee == null)
            {
                _logger.LogInfo($"Employee with id {employeeId} does not exist in the database");
                return NotFound();
            }

            var employeeDto = _mapper.Map<EmployeeDto>(employee); 

            return Ok(employeeDto);
        }

        [HttpPost(Name = "CreateEmployeeForCompany")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employeeDto)
        {
            if (employeeDto == null)
            {
                _logger.LogError("EmployeeForCreationDto object is missing in request sent from client.");
                return BadRequest("EmployeeForCreationDto object is missing in the body");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the EmployeeForCreationDto object");
                return UnprocessableEntity(ModelState);
            }

            // make sure company exists
            var company = await _repo.Company.GetCompanyAsync(companyId, trackChanges: false);

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
            await _repo.SaveAsync();

            // map to dto
            var employeeToReturn = _mapper.Map<EmployeeDto>(employee);

            // return new employee dto
            return CreatedAtRoute(
                routeName: "GetSingleEmployeeOfCompany", 
                routeValues: new { companyId = companyId, employeeId = employeeToReturn.Id }, 
                employeeToReturn);
        }

        [HttpDelete("{employeeId}", Name = "DeleteEmployeeOfCompany")]
        public async Task<IActionResult> DeleteEmployeeOfCompany(Guid companyId, Guid employeeId)
        {
            // find company
            var company = await _repo.Company.GetCompanyAsync(companyId, trackChanges: false);

            if (company == null)
            {
                _logger.LogError($"[DeleteEmployeeOfCompany] Company with id {companyId} does not exist");
                return NotFound();
            }

            // find employee
            var employee = await _repo.Employee.GetEmployeeAsync(companyId, employeeId, trackChanges: false);
            
            if (employee == null)
            {
                _logger.LogError($"[DeleteEmployeeOfCompany] Employee with id {companyId} and companyId {companyId} does not exist");
                return NotFound();
            }

            // delete employee
            _repo.Employee.DeleteEmployee(employee);

            // save
            await _repo.SaveAsync();

            // return
            return NoContent();
        }

        [HttpPut("{employeeId}", Name = "UpdateEmployeeForCompany")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid employeeId, [FromBody]
            EmployeeForUpdateDto employeeUpdateDto)
        {
            var company = await _repo.Company.GetCompanyAsync(companyId, trackChanges: false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employee = await _repo.Employee.GetEmployeeAsync(companyId, employeeId, trackChanges: true);

            if (employee == null)
            {
                _logger.LogInfo($"Employee with id: {employeeId} doesn't exist in the database.");
                return NotFound();
            }

            _mapper.Map(employeeUpdateDto, employee);

            await _repo.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{employeeId}", Name = "PartiallyUpdateEmployeeForCompany")]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid employeeId,
        [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                _logger.LogError("patchDoc object sent from client is null.");
                return BadRequest("patchDoc object is null");
            }

            var company = await _repo.Company.GetCompanyAsync(companyId, trackChanges: false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeFound = await _repo.Employee.GetEmployeeAsync(companyId, employeeId, trackChanges: true);

            if (employeeFound == null)
            {
                _logger.LogInfo($"Employee with id: {employeeId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeFound);

            patchDoc.ApplyTo(employeeToPatch, ModelState);

            TryValidateModel(employeeToPatch);

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the EmployeeForUpdateDto object");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(employeeToPatch, employeeFound);

            await _repo.SaveAsync();

            return NoContent();
        }
    }
}
