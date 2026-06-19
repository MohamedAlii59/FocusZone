using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DAL.Database;
using DAL.Entities;

namespace PL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocationsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all countries
        /// </summary>
        [HttpGet("countries")]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
        {
            var countries = await _context.Countries.ToListAsync();
            return Ok(countries);
        }

        /// <summary>
        /// Get governorates by country
        /// </summary>
        [HttpGet("countries/{countryId}/governorates")]
        public async Task<ActionResult<IEnumerable<Governorate>>> GetGovernoratesByCountry(int countryId)
        {
            var governorates = await _context.Governorates
                .Where(g => g.CountryId == countryId)
                .ToListAsync();

            if (!governorates.Any())
                return NotFound(new { message = "No governorates found for this country" });

            return Ok(governorates);
        }

        /// <summary>
        /// Get cities by governorate
        /// </summary>
        [HttpGet("governorates/{governorateId}/cities")]
        public async Task<ActionResult<IEnumerable<City>>> GetCitiesByGovernorate(int governorateId)
        {
            var cities = await _context.Cities
                .Where(c => c.GovernorateId == governorateId)
                .ToListAsync();

            if (!cities.Any())
                return NotFound(new { message = "No cities found for this governorate" });

            return Ok(cities);
        }
    }
}
