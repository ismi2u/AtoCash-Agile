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
    public class BusinessTypesController : ControllerBase
    {
        private readonly AtoCashDbContext _context;

        public BusinessTypesController(AtoCashDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        [ActionName("BusinessTypesForDropdown")]
        public async Task<ActionResult<IEnumerable<BusinessTypeVM>>> GetBusinessTypesForDropdown()
        {
            List<BusinessTypeVM> ListBusinessTypesVM = new();

            var BusinessTypes = await _context.BusinessTypes.Where(c => c.StatusTypeId == (int)EStatusType.Active).ToListAsync();
            foreach (BusinessType businessType in BusinessTypes)
            {
                BusinessTypeVM BusinessTypesVM = new()
                {
                    Id = businessType.Id,
                    BusinessTypeName = businessType.BusinessTypeName
                };

                ListBusinessTypesVM.Add(BusinessTypesVM);
            }

            return ListBusinessTypesVM;

        }
        // GET: api/BusinessTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BusinessTypeDTO>>> GetBusinessTypes()
        {
            List<BusinessTypeDTO> ListBusinessTypesDTO = new();

            var ListBusinessTypes = await _context.BusinessTypes.ToListAsync();

            foreach (BusinessType businessType in ListBusinessTypes)
            {
                BusinessTypeDTO BusinessTypesDTO = new()
                {
                    Id = businessType.Id,
                    BusinessTypeName = _context.BusinessTypes.Find(businessType.Id).BusinessTypeName,
                    BusinessTypeDesc= businessType.BusinessTypeDesc,
                    StatusTypeId = businessType.StatusTypeId,
                    StatusType = _context.StatusTypes.Find(businessType.StatusTypeId).Status
                };

                ListBusinessTypesDTO.Add(BusinessTypesDTO);

            }
            return Ok(ListBusinessTypesDTO);
        }

        // GET: api/BusinessTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BusinessTypeDTO>> GetBusinessType(int id)
        {
            var businessType = await _context.BusinessTypes.FindAsync(id);

            if (businessType == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "General Ledger Account No is Invalid!" });
            }

            BusinessTypeDTO BusinessTypesDTO = new()
            {
                Id = businessType.Id,
                BusinessTypeName = _context.BusinessTypes.Find(businessType.Id).BusinessTypeName,
                BusinessTypeDesc = businessType.BusinessTypeDesc,
                StatusTypeId = businessType.StatusTypeId,
                StatusType = _context.StatusTypes.Find(businessType.StatusTypeId).Status
            };

            return BusinessTypesDTO;
        }

        // PUT: api/BusinessTypes/5
        [HttpPut("{id}")]
    //  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> PutBusinessTypes(int id, BusinessTypeDTO BusinessTypesDTO)
        {
            if (id != BusinessTypesDTO.Id)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Id is invalid" });
            }

            var UpdBusinessType = await _context.BusinessTypes.FindAsync(id);

            if(UpdBusinessType != null)
            {
                UpdBusinessType.BusinessTypeName = BusinessTypesDTO.BusinessTypeName;
                UpdBusinessType.BusinessTypeDesc = BusinessTypesDTO.BusinessTypeDesc;
                UpdBusinessType.StatusTypeId = BusinessTypesDTO.StatusTypeId;

                _context.BusinessTypes.Update(UpdBusinessType);
            }
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Ok(new RespStatus { Status = "Success", Message = "Business Type Details Updated!" });
        }

        // POST: api/BusinessTypes
        [HttpPost]
    //  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<ActionResult<BusinessType>> PostBusinessTypes(BusinessTypeDTO BusinessTypesDTO)
        {
            var BusinessTypes = _context.BusinessTypes.Where(e => e.BusinessTypeName == BusinessTypesDTO.BusinessTypeName).FirstOrDefault();

            if (BusinessTypes != null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Business Type Name already exists" });
            }

            BusinessType newBusinessType = new();

            newBusinessType.BusinessTypeName = BusinessTypesDTO.BusinessTypeName;
            newBusinessType.BusinessTypeDesc = BusinessTypesDTO.BusinessTypeDesc;
            newBusinessType.StatusTypeId = BusinessTypesDTO.StatusTypeId;

            _context.BusinessTypes.Add(newBusinessType);

            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Business Type added to the database!" });
        }

        // DELETE: api/BusinessTypes/5
        [HttpDelete("{id}")]
    //  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> DeleteBusinessTypes(int id)
        {

            var BusinessType = await _context.BusinessTypes.FindAsync(id);
            if (BusinessType == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Business Type Invalid!" });
            }

         
            var employeesBusinessTypes = _context.BusinessUnits.Where(d => d.BusinessTypeId == id).FirstOrDefault();

            if (employeesBusinessTypes != null)
            {
               return Conflict(new RespStatus { Status = "Failure", Message = "Business Type is in use for Employees Table!" });
            }

            _context.BusinessTypes.Remove(BusinessType);
            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Business Type Account No Deleted!" });
        }

       


  
        ///


    }
}
