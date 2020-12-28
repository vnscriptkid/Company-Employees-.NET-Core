using AutoMapper;
using CompanyEmployees.ActionFilters;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> GetAllEmployeesOfCompany(Guid companyId, 
            [FromQuery] EmployeeParameters employeeParameters)
        {
            var employees = await _repo.Employee.GetEmployeesAsync(companyId, employeeParameters, trackChanges: false);

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(employees.MetaData));

            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employees).ToList();

            return Ok(employeesDto);
        }

        [HttpGet("{employeeId}", Name = "GetSingleEmployeeOfCompany")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public IActionResult GetSingleEmployeeOfCompany(Guid companyId, Guid employeeId)
        {
            var employeeOfCompany = HttpContext.Items["employee"];

            var employeeDto = _mapper.Map<EmployeeDto>(employeeOfCompany); 

            return Ok(employeeDto);
        }

        [HttpPost(Name = "CreateEmployeeForCompany")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employeeDto)
        {
            var employee = _mapper.Map<Employee>(employeeDto);

            _repo.Employee.CreateEmployee(companyId: companyId, employee: employee);

            await _repo.SaveAsync();

            var employeeToReturn = _mapper.Map<EmployeeDto>(employee);

            return CreatedAtRoute(
                routeName: "GetSingleEmployeeOfCompany", 
                routeValues: new { companyId = companyId, employeeId = employeeToReturn.Id }, 
                employeeToReturn);
        }

        [HttpDelete("{employeeId}", Name = "DeleteEmployeeOfCompany")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteEmployeeOfCompany(Guid companyId, Guid employeeId)
        {
            var employee = HttpContext.Items["employee"] as Employee;

            _repo.Employee.DeleteEmployee(employee);

            await _repo.SaveAsync();

            return NoContent();
        }

        [HttpPut("{employeeId}", Name = "UpdateEmployeeForCompany")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid employeeId, [FromBody]
            EmployeeForUpdateDto employeeUpdateDto)
        {
            var employee = HttpContext.Items["employee"] as Employee;

            _mapper.Map(employeeUpdateDto, employee);

            await _repo.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{employeeId}", Name = "PartiallyUpdateEmployeeForCompany")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid employeeId,
        [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                _logger.LogError("patchDoc object sent from client is null.");
                return BadRequest("patchDoc object is null");
            }

            var employeeFound = HttpContext.Items["employee"];

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
