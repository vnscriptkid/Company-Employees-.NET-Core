using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public CompaniesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetCompanies()
        {
            var companies = _repository.Company.GetAllCompanies(trackChanges: false);

            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);

            return Ok(companiesDto);
        }

        [HttpGet("{id}", Name = "GetCompanyById")]
        public IActionResult GetCompany(Guid id)
        {
            var company = _repository.Company.GetCompany(companyId: id, trackChanges: false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id {id} does not exist in database.");
                return NotFound();
            }

            var companyDto = _mapper.Map<CompanyDto>(company);

            return Ok(companyDto);
        }

        [HttpPost]
        public IActionResult CreateCompany([FromBody] CompanyForCreationDto companyDto)
        {
            if (companyDto == null)
            {
                _logger.LogError("CompanyForCreationDto object is null in the body.");
                return BadRequest("Company is missing in the body.");
            }

            var company = _mapper.Map<Company>(companyDto);

            _repository.Company.Create(company);

            _repository.Save();

            var companyToReturn = _mapper.Map<CompanyDto>(company);

            return CreatedAtRoute(
                routeName: "GetCompanyById",
                routeValues: new { id = companyToReturn.Id },
                value: companyToReturn
            );
        }
    }
}
