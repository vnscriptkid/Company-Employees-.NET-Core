﻿using AutoMapper;
using CompanyEmployees.ModelBinders;
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

        [HttpGet(Name = "GetAllCompanies")]
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

        [HttpPost(Name = "CreateCompany")]
        public IActionResult CreateCompany([FromBody] CompanyForCreationDto companyDto)
        {
            if (companyDto == null)
            {
                _logger.LogError("CompanyForCreationDto object is null in the body.");
                return BadRequest("Company is missing in the body.");
            }

            var company = _mapper.Map<Company>(companyDto);

            _repository.Company.CreateCompany(company);

            _repository.Save();

            var companyToReturn = _mapper.Map<CompanyDto>(company);

            return CreatedAtRoute(
                routeName: "GetCompanyById",
                routeValues: new { id = companyToReturn.Id },
                value: companyToReturn
            );
        }

        [HttpGet("collection/({ids})", Name = "CompanyCollection")]
        public IActionResult GetCompanyCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                _logger.LogError("Parameter ids is null");
                return BadRequest("Parameter ids is null");
            }
            var companies = _repository.Company.GetByIds(ids, trackChanges: false);

            if (ids.Count() != companies.Count())
            {
                _logger.LogError("Some ids are not valid in a collection");
                return NotFound();
            }

            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companies);

            return Ok(companiesToReturn);
        }

        [HttpPost("collection")]
        public IActionResult CreateCompanyCollection([FromBody]
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

            _repository.Save();

            var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companiesToCreate);

            var ids = string.Join(",", companyCollectionToReturn.Select(c => c.Id));

            return CreatedAtRoute("CompanyCollection", new { ids }, companyCollectionToReturn);
        }

        [HttpDelete("{id}", Name = "DeleteCompany")]
        public IActionResult DeleteCompany(Guid id)
        {
            // find company by id
            var company = _repository.Company.GetCompany(companyId: id, trackChanges: false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id {id} does not exist in the database.");
                return NotFound();
            }

            // delete
            _repository.Company.DeleteCompany(company);

            // save
            _repository.Save();

            return NoContent();
        }
    }
}
