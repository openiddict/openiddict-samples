using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace newoidc.Models
{
    public class result_model
    {
        public Boolean Succeeded { get; set; }
        public List<result_error_model> errors { get; set; }
    }

    public class result_error_model
    {
        public string error { get; set; }
        public string error_description { get; set; }
    }
}
