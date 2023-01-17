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
  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, User")]
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
            foreach (BusinessUnit businessUnit in BusinessUnits)
            {
                BusinessUnitVM BusinessUnitsVM = new()
                {
                    Id = businessUnit.Id,
                    BusinessUnitName = businessUnit.GetBusinessUnitName()
                };

                ListBusinessUnitsVM.Add(BusinessUnitsVM);
            }

            return ListBusinessUnitsVM;

        }


        [HttpGet("{id}")]
        [ActionName("BusinessUnitsByBusinessTypeIdForDropdown")]
        public async Task<ActionResult<IEnumerable<BusinessUnitVM>>> GetBusinessUnitsByBusinessTypeForDropdown(int id)
        {
            List<BusinessUnitVM> ListBusinessUnitsVM = new();

            var BusinessUnits = await _context.BusinessUnits.Where(c => c.BusinessTypeId == id &&  c.StatusTypeId == (int)EStatusType.Active).ToListAsync();
            foreach (BusinessUnit businessUnit in BusinessUnits)
            {
                BusinessUnitVM BusinessUnitsVM = new()
                {
                    Id = businessUnit.Id,
                    BusinessUnitName = businessUnit.GetBusinessUnitName()
                };

                ListBusinessUnitsVM.Add(BusinessUnitsVM);
            }

            return ListBusinessUnitsVM;

        }


        [HttpPost]
        [ActionName("BusinessUnitsByBizTypeIdAndEmpIdForDropdown")]
        public async Task<ActionResult<IEnumerable<BusinessUnitVM>>> GetBusinessUnitsByBizTypeIdAndEmpIdForDropdown(BusinessTypeIdAndEmpId bizIdAndEmpID )
        {
            List<BusinessUnitVM> ListBusinessUnitsVM = new();

           

            var ListEmployeeExtendedInfo = await _context.EmployeeExtendedInfos.Where(c => c.BusinessTypeId == bizIdAndEmpID.BusinessTypeId && c.EmployeeId == bizIdAndEmpID.EmpId && c.StatusTypeId == (int)EStatusType.Active).ToListAsync();

            foreach (EmployeeExtendedInfo employeeExtendedInfo in ListEmployeeExtendedInfo)
            {
                BusinessUnitVM BusinessUnitsVM = new()
                {
                    Id = employeeExtendedInfo.BusinessUnitId,
                    BusinessUnitName = _context.BusinessUnits.Find(employeeExtendedInfo.BusinessUnitId).GetBusinessUnitName()
                };

                ListBusinessUnitsVM.Add(BusinessUnitsVM);
            }

            return ListBusinessUnitsVM;

        }

        [HttpGet("{id}")]
        [ActionName("BusinessUnitsForDropdownByCostCentre")]
        public async Task<ActionResult<IEnumerable<BusinessUnitVM>>> GetBusinessUnitsForDropdownByCostCentre(int id)
        {
            List<BusinessUnitVM> ListBusinessUnitVM = new List<BusinessUnitVM>();

            var businessUnits = await _context.BusinessUnits.Where(d => d.StatusTypeId == (int)EStatusType.Active && d.CostCenterId == id).ToListAsync();
            foreach (BusinessUnit businessUnit in businessUnits)
            {
                BusinessUnitVM businessUnitVM = new BusinessUnitVM
                {
                    Id = businessUnit.Id,
                    BusinessUnitName = businessUnit.GetBusinessUnitName()

                };

                 ListBusinessUnitVM.Add(businessUnitVM);

            };

            return ListBusinessUnitVM;
        }


    // GET: api/BusinessUnits
    [HttpGet]
        public async Task<ActionResult<IEnumerable<BusinessUnitDTO>>> GetBusinessUnits()
        {
            List<BusinessUnitDTO> ListBusinessUnitsDTO = new();

            var ListBusinessUnits = await _context.BusinessUnits.ToListAsync();

            foreach (BusinessUnit businessUnit in ListBusinessUnits)
            {
                BusinessUnitDTO BusinessUnitsDTO = new()
                {
                    Id = businessUnit.Id,
                    BusinessTypeId = businessUnit.BusinessTypeId,
                    BusinessType = _context.BusinessTypes.Find(businessUnit.BusinessTypeId).BusinessTypeName,
                    BusinessUnitCode= businessUnit.BusinessUnitCode,
                    BusinessUnitName = businessUnit.BusinessUnitName,
                    CostCenterId = businessUnit.CostCenterId,
                    CostCenter = _context.CostCenters.Find(businessUnit.CostCenterId).GetCostCentre(),
                    BusinessDesc = businessUnit.BusinessDesc,
                    LocationId = businessUnit.LocationId,
                    Location = _context.Locations.Find(businessUnit.LocationId).LocationName,
                    StatusTypeId = businessUnit.StatusTypeId,
                    StatusType = _context.StatusTypes.Find(businessUnit.StatusTypeId).Status
                };

                ListBusinessUnitsDTO.Add(BusinessUnitsDTO);

            }
            return Ok(ListBusinessUnitsDTO);
        }

        // GET: api/BusinessUnits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BusinessUnitDTO>> GetBusinessUnit(int id)
        {
            var businessUnit = await _context.BusinessUnits.FindAsync(id);

            if (businessUnit == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "General Ledger Account No is Invalid!" });
            }

            BusinessUnitDTO BusinessUnitsDTO = new()
            {
                Id = businessUnit.Id,
                BusinessTypeId = businessUnit.BusinessTypeId,
                BusinessType = _context.BusinessTypes.Find(businessUnit.BusinessTypeId).BusinessTypeName,
                BusinessUnitCode = businessUnit.BusinessUnitCode,
                BusinessUnitName = businessUnit.BusinessUnitName,
                CostCenterId = businessUnit.CostCenterId,
                CostCenter = _context.CostCenters.Find(businessUnit.CostCenterId).GetCostCentre(),
                BusinessDesc = businessUnit.BusinessDesc,
                LocationId = businessUnit.LocationId,
                Location = _context.Locations.Find(businessUnit.LocationId).LocationName,
                StatusTypeId = businessUnit.StatusTypeId,
                StatusType = _context.StatusTypes.Find(businessUnit.StatusTypeId).Status
            };

            return BusinessUnitsDTO;
        }

        // PUT: api/BusinessUnits/5
        [HttpPut("{id}")]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> PutBusinessUnits(int id, BusinessUnitDTO BusinessUnitsDTO)
        {
            if (id != BusinessUnitsDTO.Id)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Id is invalid" });
            }

            var UpdBusinessUnit = await _context.BusinessUnits.FindAsync(id);

            if(UpdBusinessUnit != null)
            {
                UpdBusinessUnit.BusinessTypeId = BusinessUnitsDTO.BusinessTypeId;
                UpdBusinessUnit.BusinessUnitName = BusinessUnitsDTO.BusinessUnitName;
                UpdBusinessUnit.BusinessUnitCode = BusinessUnitsDTO.BusinessUnitCode;

                bool isUsedInExpReim = _context.ExpenseReimburseRequests.Where(x => x.CostCenterId == BusinessUnitsDTO.CostCenterId && x.BusinessTypeId == BusinessUnitsDTO.BusinessTypeId).Any();
                bool isUsedInCashAdv = _context.CashAdvanceRequests.Where(x => x.CostCenterId == BusinessUnitsDTO.CostCenterId && x.BusinessTypeId == BusinessUnitsDTO.BusinessTypeId).Any();
                bool isUsedInTravelReq = _context.TravelApprovalRequests.Where(x => x.CostCenterId == BusinessUnitsDTO.CostCenterId && x.BusinessTypeId == BusinessUnitsDTO.BusinessTypeId).Any();

                if (!(isUsedInExpReim || isUsedInCashAdv || isUsedInTravelReq)) // is not used in any of the 
                {
                    UpdBusinessUnit.CostCenterId = BusinessUnitsDTO.CostCenterId;
                } 
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
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<ActionResult<BusinessUnit>> PostBusinessUnits(BusinessUnitDTO BusinessUnitsDTO)
        {
            var BusinessUnits = _context.BusinessUnits.Where(e => e.BusinessUnitName == BusinessUnitsDTO.BusinessUnitName && e.BusinessTypeId == BusinessUnitsDTO.BusinessTypeId).FirstOrDefault();

            if (BusinessUnits != null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "BusinessUnits Name already exists" });
            }

            BusinessUnit newBusinessUnit = new();

            newBusinessUnit.BusinessUnitName = BusinessUnitsDTO.BusinessUnitName;
            newBusinessUnit.BusinessUnitCode= BusinessUnitsDTO.BusinessUnitCode;
            newBusinessUnit.BusinessTypeId = BusinessUnitsDTO.BusinessTypeId;
            newBusinessUnit.CostCenterId = BusinessUnitsDTO.CostCenterId;
            newBusinessUnit.BusinessDesc = BusinessUnitsDTO.BusinessDesc;
            newBusinessUnit.LocationId = BusinessUnitsDTO.LocationId;
            newBusinessUnit.StatusTypeId = BusinessUnitsDTO.StatusTypeId;

            _context.BusinessUnits.Add(newBusinessUnit);

            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Business Unit Created!" });
        }

        // DELETE: api/BusinessUnits/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
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
