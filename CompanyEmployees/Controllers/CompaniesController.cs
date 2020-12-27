using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
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

        [HttpGet(Name = "GetAllCompanies")]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _repository.Company.GetAllCompaniesAsync(trackChanges: false);

            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);

            return Ok(companiesDto);
        }

        [HttpGet("{companyId}", Name = "GetCompanyById")]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public IActionResult GetCompany(Guid companyId)
        {
            var company = HttpContext.Items["company"] as Company;

            var companyDto = _mapper.Map<CompanyDto>(company);

            return Ok(companyDto);
        }

        [HttpPost(Name = "CreateCompany")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto companyDto)
        {
            var company = _mapper.Map<Company>(companyDto);

            _repository.Company.CreateCompany(company);

            await _repository.SaveAsync();

            var companyToReturn = _mapper.Map<CompanyDto>(company);

            return CreatedAtRoute(
                routeName: "GetCompanyById",
                routeValues: new { companyId = companyToReturn.Id },
                value: companyToReturn
            );
        }

        [HttpGet("collection/({ids})", Name = "CompanyCollection")]
        public async Task<IActionResult> GetCompanyCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                _logger.LogError("Parameter ids is null");
                return BadRequest("Parameter ids is null");
            }
            var companies = await _repository.Company.GetByIdsAsync(ids, trackChanges: false);

            if (ids.Count() != companies.Count())
            {
                _logger.LogError("Some ids are not valid in a collection");
                return NotFound();
            }

            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companies);

            return Ok(companiesToReturn);
        }

        [HttpPost("collection", Name = "CreateCompanyCollection")]
        public async Task<IActionResult> CreateCompanyCollection([FromBody]
            IEnumerable<CompanyForCreationDto> companyCollection)
        {
            if (companyCollection == null)
            {
                _logger.LogError("Company collection sent from client is null.");
                return BadRequest("Company collection is null");
            }

            var companiesToCreate = _mapper.Map<IEnumerable<Company>>(companyCollection);

            foreach (var company in companiesToCreate)
            {
                _repository.Company.CreateCompany(company);
            }

            await _repository.SaveAsync();

            var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companiesToCreate);

            var ids = string.Join(",", companyCollectionToReturn.Select(c => c.Id));

            return CreatedAtRoute("CompanyCollection", new { ids }, companyCollectionToReturn);
        }

        [HttpDelete("{companyId}", Name = "DeleteCompany")]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteCompany(Guid companyId)
        {
            var company = HttpContext.Items["company"] as Company;

            _repository.Company.DeleteCompany(company);

            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPut("{companyId}", Name = "UpdateCompany")]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateCompany(Guid companyId, [FromBody] CompanyForUpdateDto companyUpdateDto)
        {
            var companyToUpdate = HttpContext.Items["company"] as Company;

            _mapper.Map(companyUpdateDto, companyToUpdate);

            await _repository.SaveAsync();

            return NoContent();
        }
    }
}
