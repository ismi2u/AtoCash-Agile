using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AtoCashAPI.Models
{
    public class AccountPayableMapping
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Id")]
        public virtual CostCenter? CostCenter { get; set; }
        public int CostCenterId { get; set; }

        [Required]
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
        public int EmployeeId { get; set; }

       

    }

    public class AccountPayableMappingDTO
    {
        public int Id { get; set; }

        public int CostCenterId { get; set; }
        public string? CostCenterCode { get; set; }
        public string? CostCenterDesc { get; set; }

        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }

        
    }

    public class GetEmployeesForCostCenter
    {
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public bool isAssigned { get; set; }

    }

    public class AddEmployeesToAccountPayableMapping
    {
        public int CostCenterId { get; set; }
        public List<int>? EmployeeIds { get; set; }

    }

    public class AccountPayableMappingVM
    {
        public int Id { get; set; }
        public string? CostCenterCode { get; set; }
        public string? CostCenterDesc { get; set; }

    }

    public class AddEmployeesToCostCenter
    {
        public int CostCenterId { get; set; }
        public List<int>? EmployeeIds { get; set; }
    }


    public class EmployeeByRole : EmployeeDTO
    {
        public string? UserId { get; set; }
        public string? UserFullName { get; set; }
        public new string? StatusType { get; set; }
        public string? AccessRole { get; set; }
    }

}
