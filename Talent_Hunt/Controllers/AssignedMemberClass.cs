using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talent_Hunt.Controllers
{
   
        public class AssignedMemberClass
        {
            public int EventID { get; set; }
            public List<int> MemberIdlist { get; set; }
            public string Status { get; set; }
        }

    }