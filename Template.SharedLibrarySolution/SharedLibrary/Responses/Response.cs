using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Responses
{
    public record Response
    {
        public bool etat {  get; set; }
        public string? message { get; set; }

        public Response(bool v, string token)
        {
            this.etat = v;
            this.message = token;
        }
    }
}
