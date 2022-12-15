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
    public class BusinessUnitsController : ControllerBase
    {
        private readonly AtoCashDbContext _context;

        public BusinessUnitsController(AtoCashDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        [ActionName("BusinessUnitsForDropdown")]
        public async Task<ActionResult<IEnumerable<BusinessUnitVM>>> GetBusinessUnitsForDropdown()
        {
            List<BusinessUnitVM> ListBusinessUnitsVM = new();

            var BusinessUnits = await _context.BusinessUnits.Where(c => c.StatusTypeId == (int)EStatusType.Active).ToListAsync();
            foreach (BusinessUnit bizUnit in BusinessUnits)
            {
                BusinessUnitVM BusinessUnitsVM = new()
                {
                    Id = bizUnit.Id,
                    BusinessUnit = bizUnit.BusinessUnitType = ":" + bizUnit.BusinessUnitName
                };

                ListBusinessUnitsVM.Add(BusinessUnitsVM);
            }

            return ListBusinessUnitsVM;

        }
        // GET: api/BusinessUnits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BusinessUnitDTO>>> GetBusinessUnitss()
        {
            List<BusinessUnitDTO> ListBusinessUnitsDTO = new();

            var BusinessUnitss = await _context.BusinessUnits.ToListAsync();

            foreach (BusinessUnit BusinessUnits in BusinessUnitss)
            {
                BusinessUnitDTO BusinessUnitsDTO = new()
                {
                    Id = BusinessUnits.Id,
                    BusinessUnitType = BusinessUnits.BusinessUnitType,
                    BusinessUnitName = BusinessUnits.BusinessUnitName,
                    CostCenterId = BusinessUnits.CostCenterId,
                    CostCenter = _context.CostCenters.Find(BusinessUnits.CostCenterId).GetCostCentre(),
                    BusinessDesc = BusinessUnits.BusinessDesc,
                    LocationId = BusinessUnits.LocationId,
                    Location = _context.Locations.Find(BusinessUnits.LocationId).LocationName,
                    StatusTypeId = BusinessUnits.StatusTypeId,
                    StatusType = _context.StatusTypes.Find(BusinessUnits.StatusTypeId).Status
                };

                ListBusinessUnitsDTO.Add(BusinessUnitsDTO);

            }
            return Ok(ListBusinessUnitsDTO);
        }

        // GET: api/BusinessUnits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BusinessUnitDTO>> GetBusinessUnits(int id)
        {
            var BusinessUnits = await _context.BusinessUnits.FindAsync(id);

            if (BusinessUnits == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "General Ledger Account No is Invalid!" });
            }

            BusinessUnitDTO BusinessUnitsDTO = new()
            {
                Id = BusinessUnits.Id,
                BusinessUnitType = BusinessUnits.BusinessUnitType,
                BusinessUnitName = BusinessUnits.BusinessUnitName,
                CostCenterId = BusinessUnits.CostCenterId,
                CostCenter = _context.CostCenters.Find(BusinessUnits.CostCenterId).GetCostCentre(),
                BusinessDesc = BusinessUnits.BusinessDesc,
                LocationId = BusinessUnits.LocationId,
                Location = _context.Locations.Find(BusinessUnits.LocationId).LocationName,
                StatusTypeId = BusinessUnits.StatusTypeId,
                StatusType = _context.StatusTypes.Find(BusinessUnits.StatusTypeId).Status
            };

            return BusinessUnitsDTO;
        }

        // PUT: api/BusinessUnits/5
        [HttpPut("{id}")]
    //  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> PutBusinessUnits(int id, BusinessUnitDTO BusinessUnitsDTO)
        {
            if (id != BusinessUnitsDTO.Id)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Id is invalid" });
            }

            var UpdBusinessUnit = await _context.BusinessUnits.FindAsync(id);

            if(UpdBusinessUnit != null)
            {
                UpdBusinessUnit.BusinessUnitType = BusinessUnitsDTO.BusinessUnitType;
                UpdBusinessUnit.BusinessUnitName = BusinessUnitsDTO.BusinessUnitName;
                UpdBusinessUnit.CostCenterId = BusinessUnitsDTO.CostCenterId;
                UpdBusinessUnit.BusinessDesc = BusinessUnitsDTO.BusinessDesc;
                UpdBusinessUnit.LocationId = BusinessUnitsDTO.LocationId;
                UpdBusinessUnit.StatusTypeId = BusinessUnitsDTO.StatusTypeId;

                _context.BusinessUnits.Update(UpdBusinessUnit);
            }
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Ok(new RespStatus { Status = "Success", Message = "Business Unit Details Updated!" });
        }

        // POST: api/BusinessUnits
        [HttpPost]
    //  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<ActionResult<BusinessUnit>> PostBusinessUnits(BusinessUnitDTO BusinessUnitsDTO)
        {
            var BusinessUnits = _context.BusinessUnits.Where(e => e.BusinessUnitType == BusinessUnitsDTO.BusinessUnitType && e.BusinessUnitName == BusinessUnitsDTO.BusinessUnitName).FirstOrDefault();

            if (BusinessUnits != null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "BusinessUnits Name already exists" });
            }

            BusinessUnit newBusinessUnit = new();

            newBusinessUnit.BusinessUnitType = BusinessUnitsDTO.BusinessUnitType;
            newBusinessUnit.BusinessUnitName = BusinessUnitsDTO.BusinessUnitName;
            newBusinessUnit.CostCenterId = BusinessUnitsDTO.CostCenterId;
            newBusinessUnit.BusinessDesc = BusinessUnitsDTO.BusinessDesc;
            newBusinessUnit.LocationId = BusinessUnitsDTO.LocationId;
            newBusinessUnit.StatusTypeId = BusinessUnitsDTO.StatusTypeId;

            _context.BusinessUnits.Add(newBusinessUnit);

            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Business Unit added to the database!" });
        }

        // DELETE: api/BusinessUnits/5
        [HttpDelete("{id}")]
    //  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> DeleteBusinessUnits(int id)
        {

            var BusinessUnits = await _context.BusinessUnits.FindAsync(id);
            if (BusinessUnits == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Business Unit Invalid!" });
            }

         
            var employeesBusinessUnits = _context.EmployeeExtendedInfos.Where(d => d.BusinessUnitId == id).FirstOrDefault();

            if (employeesBusinessUnits != null)
            {
               return Conflict(new RespStatus { Status = "Failure", Message = "Business Unit is in use for Employees Table!" });
            }

            _context.BusinessUnits.Remove(BusinessUnits);
            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Business Unit Account No Deleted!" });
        }

       


  
        ///


    }
}
