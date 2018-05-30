using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace GoSqlApi.Controllers
{
    [Route("api/[controller]")]
    public class DataController : Controller
    {

        // GET api/data/var001
        [HttpGet("{id}")]
        public string Get(string id)
        {
            ApiHelper helper = new ApiHelper();
            string data = helper.ExecuteCommand("GET", id);

            CheckForErrors(data);

            return data;
        }

        // POST api/data
        [HttpPost]
        public string Post([FromBody]string id, [FromBody]string value)
        {
            ApiHelper helper = new ApiHelper();
            string data = helper.ExecuteCommand("CREATE", id, value);

            CheckForErrors(data);

            return data;
        }

        // PUT api/data/var001
        [HttpPut("{id}")]
        public string Put(string id, [FromBody]string value)
        {
            ApiHelper helper = new ApiHelper();
            string data = helper.ExecuteCommand("UPDATE", id, value);

            CheckForErrors(data);

            return data;
        }

        // DELETE api/data/var001
        [HttpDelete("{id}")]
        public string Delete(string id)
        {
            ApiHelper helper = new ApiHelper();
            string data = helper.ExecuteCommand("DELETE", id);

            CheckForErrors(data);

            return data;
        }

        #region Error Handling Methods
        private void CheckForErrors(string data)
        {
            if (data.Contains("ERROR!")) ThrowResponseException(data);
        }

        private void ThrowResponseException(string message)
        {
            HttpResponseMessage errorResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = message
            };
            throw new System.Web.Http.HttpResponseException(errorResponse);
        }

        #endregion
    }
}
