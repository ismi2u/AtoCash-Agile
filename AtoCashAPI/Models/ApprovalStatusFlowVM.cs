using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Models
{
    public class ApprovalStatusFlowVM
    {
        public int? ApprovalLevel { get; set; }
        public string? ApproverRole { get; set; }
        public string? ApproverName { get; set; }
        
        public string? ApprovalStatusType { get; set; }
        public DateTime? ApproverActionDate { get; set; }

    }
}
