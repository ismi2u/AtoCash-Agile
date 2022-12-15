using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtoCashAPI.Data;
using AtoCashAPI.Models;
using Microsoft.AspNetCore.Authorization;
using AtoCashAPI.Authentication;

namespace AtoCashAPI.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
//  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, User")]
    public class LocationsController : ControllerBase
    {
        private readonly AtoCashDbContext _context;

        public LocationsController(AtoCashDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ActionName("LocationsForDropdown")]
        public async Task<ActionResult<IEnumerable<LocationVM>>> GetLocationsForDropDown()
        {
            List<LocationVM> ListLocationVM = new List<LocationVM>();

            var Locations = await _context.Locations.Where(c => c.StatusTypeId == (int)EStatusType.Active).ToListAsync();
            foreach (Location Location in Locations)
            {
                LocationVM LocationVM = new LocationVM
                {
                    Id = Location.Id,
                    LocationName = Location.LocationName + " " + Location.City,
                };

                ListLocationVM.Add(LocationVM);
            }

            return ListLocationVM;

        }

        // GET: api/Locations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationDTO>>> GetLocations()
        {

            List<LocationDTO> ListLocationDTO = new List<LocationDTO>();

            var Locations = await _context.Locations.ToListAsync();

            foreach (Location Location in Locations)
            {
                LocationDTO LocationDTO = new LocationDTO
                {
                    Id = Location.Id,
                    LocationName = Location.LocationName,
                    City = Location.City,
                    Lattitude = Location.Lattitude,
                    Longitude = Location.Longitude,
                    LocationDesc = Location.LocationDesc,
                    StatusTypeId = Location.StatusTypeId,
                    StatusType = _context.StatusTypes.Find(Location.StatusTypeId).Status
                };

                ListLocationDTO.Add(LocationDTO);

            }
            return Ok(ListLocationDTO);
        }

        // GET: api/Locations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LocationDTO>> GetLocation(int id)
        {
            var Location = await _context.Locations.FindAsync(id);

            if (Location == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Location Id invalid!" });
            }

            LocationDTO LocationDTO = new LocationDTO
            {
                Id = Location.Id,
                LocationName = Location.LocationName,
                City = Location.City,
                Lattitude = Location.Lattitude,
                Longitude = Location.Longitude,
                LocationDesc = Location.LocationDesc,
                StatusTypeId = Location.StatusTypeId,
                StatusType = _context.StatusTypes.Find(Location.StatusTypeId).Status
            };

            return LocationDTO;
        }

        // PUT: api/Locations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
    //  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> PutLocation(int id, LocationDTO LocationDTO)
        {
            if (id != LocationDTO.Id)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Id is invalid" });
            }

            var location = await _context.Locations.FindAsync(id);

            if (location != null)
            {
                location.LocationName = LocationDTO.LocationName;
                location.City = LocationDTO.City;
                location.Lattitude = LocationDTO.Lattitude;
                location.Longitude = LocationDTO.Longitude;
                location.LocationDesc = LocationDTO.LocationDesc;
                location.StatusTypeId = LocationDTO.StatusTypeId;

                _context.Locations.Update(location);
            }

            //_context.Entry(Location).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocationExists(id))
                {
                    return Conflict(new RespStatus { Status = "Failure", Message = "Location is invalid" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new RespStatus { Status = "Success", Message = "Location Details Updated!" });
        }

        // POST: api/Locations
        [HttpPost]
    //  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<ActionResult<Location>> PostLocation(LocationDTO LocationDTO)
        {
            var location = _context.Locations.Where(c => c.LocationName == LocationDTO.LocationName).FirstOrDefault();
            if (location != null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Location already exists" });
            }
            Location Newlocation = new();

            Newlocation.LocationName = LocationDTO.LocationName;
            Newlocation.City = LocationDTO.City;
            Newlocation.Lattitude = LocationDTO.Lattitude;
            Newlocation.Longitude = LocationDTO.Longitude;
            Newlocation.LocationDesc = LocationDTO.LocationDesc;
            Newlocation.StatusTypeId = LocationDTO.StatusTypeId;

            _context.Locations.Add(Newlocation);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
               return Conflict(new RespStatus { Status = "Failure", Message = "Location creation failed!" });
            }

            return Ok(new RespStatus { Status = "Success", Message = "Location Created!" });
        }

        // DELETE: api/Locations/5
        [HttpDelete("{id}")]
    //  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var bussUnit = _context.BusinessUnits.Where(d => d.LocationId == id).FirstOrDefault();

            if (bussUnit != null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Location in use for Business Unit(s)" });
            }
           

            var Location = await _context.Locations.FindAsync(id);
            if (Location == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Location Id invalid!" });
            }

            _context.Locations.Remove(Location);
            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Location Deleted!" });
        }

        private bool LocationExists(int id)
        {
            return _context.Locations.Any(e => e.Id == id);
        }


     
        //
    }
}
