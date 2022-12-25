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
    public class VendorsController : ControllerBase
    {
        private readonly AtoCashDbContext _context;

        public VendorsController(AtoCashDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ActionName("VendorsForDropdown")]
        public async Task<ActionResult<IEnumerable<VendorVM>>> GetVendorsForDropDown()
        {
            List<VendorVM> ListVendorVM = new List<VendorVM>();

            var Vendors = await _context.Vendors.Where(c => c.StatusTypeId == (int)EStatusType.Active).ToListAsync();
            foreach (Vendor Vendor in Vendors)
            {
                VendorVM VendorVM = new VendorVM
                {
                    Id = Vendor.Id,
                    VendorName = Vendor.VendorName + " " + Vendor.City,
                };

                ListVendorVM.Add(VendorVM);
            }

            return ListVendorVM;

        }

        // GET: api/Vendors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VendorDTO>>> GetVendors()
        {

            List<VendorDTO> ListVendorDTO = new List<VendorDTO>();

            var Vendors = await _context.Vendors.ToListAsync();

            foreach (Vendor Vendor in Vendors)
            {
                VendorDTO VendorDTO = new VendorDTO
                {
                    Id = Vendor.Id,
                    VendorName = Vendor.VendorName,
                    City = Vendor.City,
                    Description = Vendor.Description,
                    StatusTypeId = Vendor.StatusTypeId,
                    StatusType = _context.StatusTypes.Find(Vendor.StatusTypeId).Status
                };

                ListVendorDTO.Add(VendorDTO);

            }
            return Ok(ListVendorDTO);
        }

        // GET: api/Vendors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VendorDTO>> GetVendor(int id)
        {
            var Vendor = await _context.Vendors.FindAsync(id);

            if (Vendor == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Vendor Id invalid!" });
            }

            VendorDTO VendorDTO = new VendorDTO
            {
                Id = Vendor.Id,
                VendorName = Vendor.VendorName,
                City = Vendor.City,
                Description = Vendor.Description,
                StatusTypeId = Vendor.StatusTypeId,
                StatusType = _context.StatusTypes.Find(Vendor.StatusTypeId).Status
            };

            return VendorDTO;
        }

        // PUT: api/Vendors/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> PutVendor(int id, VendorDTO VendorDTO)
        {
            if (id != VendorDTO.Id)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Id is invalid" });
            }

            var Vendor = await _context.Vendors.FindAsync(id);

            if (Vendor != null)
            {
                Vendor.VendorName = VendorDTO.VendorName;
                Vendor.City = VendorDTO.City;
                Vendor.Description = VendorDTO.Description;
                Vendor.StatusTypeId = VendorDTO.StatusTypeId;

                _context.Vendors.Update(Vendor);
            }

            //_context.Entry(Vendor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VendorExists(id))
                {
                    return Conflict(new RespStatus { Status = "Failure", Message = "Vendor is invalid" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new RespStatus { Status = "Success", Message = "Vendor Details Updated!" });
        }

        // POST: api/Vendors
        [HttpPost]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<ActionResult<Vendor>> PostVendor(VendorDTO VendorDTO)
        {
            var Vendor = _context.Vendors.Where(c => c.VendorName == VendorDTO.VendorName).FirstOrDefault();
            if (Vendor != null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Vendor already exists" });
            }
            Vendor NewVendor = new();

            NewVendor.VendorName = VendorDTO.VendorName;
            NewVendor.City = VendorDTO.City;
            NewVendor.Description = VendorDTO.Description;
            NewVendor.StatusTypeId = VendorDTO.StatusTypeId;

            _context.Vendors.Add(NewVendor);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
               return Conflict(new RespStatus { Status = "Failure", Message = "Vendor creation failed!" });
            }

            return Ok(new RespStatus { Status = "Success", Message = "Vendor Created!" });
        }

        // DELETE: api/Vendors/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> DeleteVendor(int id)
        {
            var VendorInExpenseSubClaim = _context.ExpenseSubClaims.Where(d => d.VendorId == id).FirstOrDefault();

            if (VendorInExpenseSubClaim != null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Vendor in use for Expense Reimbursement" });
            }
           

            var Vendor = await _context.Vendors.FindAsync(id);
            if (Vendor == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Vendor Id invalid!" });
            }

            _context.Vendors.Remove(Vendor);
            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Vendor Deleted!" });
        }

        private bool VendorExists(int id)
        {
            return _context.Vendors.Any(e => e.Id == id);
        }


     
        //
    }
}
