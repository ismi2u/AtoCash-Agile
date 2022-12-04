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
  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, Manager, User")]
    public class StoresController : ControllerBase
    {
        private readonly AtoCashDbContext _context;

        public StoresController(AtoCashDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        [ActionName("StoresForDropdown")]
        public async Task<ActionResult<IEnumerable<StoreVM>>> GetStoresForDropdown()
        {
            List<StoreVM> ListStoreVM = new List<StoreVM>();

            var Stores = await _context.Stores.Where(d => d.StatusTypeId == (int)EStatusType.Active).ToListAsync();
            foreach (Store Store in Stores)
            {
                StoreVM StoreVM = new StoreVM
                {
                    Id = Store.Id,
                    StoreName = Store.StoreCode + ":" + Store.StoreName
                };

                ListStoreVM.Add(StoreVM);
            }
            return ListStoreVM;

        }
        [HttpGet("{id}")]
        [ActionName("StoresForDropdownByCostCentre")]
        public async Task<ActionResult<IEnumerable<StoreVM>>> GetStoresForDropdownByCostCentre(int id)
        {
            List<StoreVM> ListStoreVM = new List<StoreVM>();

            var Stores = await _context.Stores.Where(d => d.StatusTypeId == (int)EStatusType.Active && d.CostCenterId == id).ToListAsync();
            foreach (Store Store in Stores)
            {
                StoreVM StoreVM = new StoreVM
                {
                    Id = Store.Id,
                    StoreName = Store.StoreCode + ":" + Store.StoreName
                };

                ListStoreVM.Add(StoreVM);
            }
            return ListStoreVM;

        }

        // GET: api/Stores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StoreDTO>>> GetStores()
        {
            List<StoreDTO> ListStoreDTO = new List<StoreDTO>();

            var Stores = await _context.Stores.ToListAsync();

            foreach (Store Store in Stores)
            {
                StoreDTO StoreDTO = new StoreDTO
                {
                    Id = Store.Id,
                    StoreCode = Store.StoreCode,
                    StoreName = Store.StoreName,
                    CostCenterId = Store.CostCenterId,
                    CostCenter = _context.CostCenters.Find(Store.CostCenterId).CostCenterCode,
                    StatusTypeId = Store.StatusTypeId,
                    StatusType = _context.StatusTypes.Find(Store.StatusTypeId).Status

                };

                ListStoreDTO.Add(StoreDTO);

            }

            return ListStoreDTO;
        }

        // GET: api/Stores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StoreDTO>> GetStore(int id)
        {
            StoreDTO StoreDTO = new StoreDTO();

            var Store = await _context.Stores.FindAsync(id);

            if (Store == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Store Id invalid!" });
            }

            StoreDTO.Id = Store.Id;
            StoreDTO.StoreCode = Store.StoreCode;
            StoreDTO.StoreName = Store.StoreName;
            StoreDTO.CostCenterId = Store.CostCenterId;
            StoreDTO.CostCenter = _context.CostCenters.Find(Store.CostCenterId).CostCenterCode;
            StoreDTO.StatusTypeId = Store.StatusTypeId;
            StoreDTO.StatusType = _context.StatusTypes.Find(Store.StatusTypeId).Status;

            return StoreDTO;
        }

        // PUT: api/Stores/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
      [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> PutStore(int id, StoreDTO StoreDto)
        {
            if (id != StoreDto.Id)
            {
                return Conflict(new Authentication.RespStatus { Status = "Failure", Message = "Id not Valid for Store" });
            }

            var Store = await _context.Stores.FindAsync(id);

            Store.StoreName = StoreDto.StoreName;
            Store.CostCenterId = StoreDto.CostCenterId;
            Store.StatusTypeId = StoreDto.StatusTypeId;

            _context.Stores.Update(Store);
            //_context.Entry(projectDto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StoreExists(id))
                {
                    return Conflict(new RespStatus { Status = "Failure", Message = "Store is invalid" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new RespStatus { Status = "Success", Message = "Store Details Updated!" });
        }

        // POST: api/Stores
        [HttpPost]
      [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<ActionResult<Store>> PostStore(StoreDTO StoreDto)
        {
            var dept = _context.Stores.Where(c => c.StoreCode == StoreDto.StoreCode).FirstOrDefault();
            if (dept != null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Store already exists" });
            }

            Store Store = new Store
            {
                StoreCode = StoreDto.StoreCode,
                StoreName = StoreDto.StoreName,
                CostCenterId = StoreDto.CostCenterId,
                StatusTypeId = StoreDto.StatusTypeId
            };

            _context.Stores.Add(Store);
            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Store Created!" });
        }

        // DELETE: api/Stores/5
        [HttpDelete("{id}")]
      [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> DeleteStore(int id)
        {

            var emp = _context.Employees.Where(e => e.StoreId == id).FirstOrDefault();

            if (emp != null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Store in Use - Can't delete" });
            }


            var Store = await _context.Stores.FindAsync(id);
            if (Store == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Store Id invalid!" });
            }

            _context.Stores.Remove(Store);
            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Store Deleted!" });
        }

        private bool StoreExists(int id)
        {
            return _context.Stores.Any(e => e.Id == id);
        }


        //
    }
}
