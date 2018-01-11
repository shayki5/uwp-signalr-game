using Newtonsoft.Json;
using ServerSide.DAL;
using ServerSide.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Windows.Data.Json;

namespace ServerSide.Controllers
{
    public class UserController : ApiController
    {
        UserRepository _rep = new UserRepository();

        // GET: api/user
        [HttpGet]
        [ActionName("getAll")]
        public IEnumerable<User> Get()
        {
            return _rep.GetAll();
        }

        // GET api/user/5
        [HttpGet]
        public DAL.User Get(int id)
        {
            return _rep.GetUser(id);
        }

        // POST api/user
        [HttpPost]
        [ActionName("register")]
        public async Task<HttpResponseMessage> register()
        {
            byte[] parms = await Request.Content.ReadAsByteArrayAsync();
            string jsonStr = Encoding.UTF8.GetString(parms);

            var user = JsonConvert.DeserializeObject<User>(jsonStr); // Convert JSON to Users
            Task.WaitAll();
            var newUser = _rep.CreateUser(user);

            if (newUser == null)
            {
                return Request.CreateResponse(HttpStatusCode.Created, "exist");
            }
            return Request.CreateResponse(HttpStatusCode.Created, newUser);
        }

        [HttpPost]
        [ActionName("checkUserValidation")]
        public async Task<HttpResponseMessage> checkUserValidation()
        {
            byte[] parms = await Request.Content.ReadAsByteArrayAsync();
            string jsonStr = Encoding.UTF8.GetString(parms);

            var user = JsonConvert.DeserializeObject<DAL.User>(jsonStr); // Convert JSON to Users
            DAL.User currentUser = new DAL.User();
            if (_rep.IsValid(user, ref currentUser))
            {
                return Request.CreateResponse(HttpStatusCode.Created, currentUser);
            }

            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "error");
            }

        }

        [HttpPost]
        [ActionName("offlineUser")]
        public async Task<HttpResponseMessage> offlineUser()
        {
            byte[] parms = await Request.Content.ReadAsByteArrayAsync();
            string jsonStr = Encoding.UTF8.GetString(parms);

            var user = JsonConvert.DeserializeObject<DAL.User>(jsonStr); // Convert JSON to Users

            _rep.OfflineUser(user);

            return  Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpGet]
        [ActionName("getOnline")]
        public IEnumerable<User> getOnline()
        {
            return _rep.GetAllOnline();
        }

        [HttpGet]
        [ActionName("getOffline")]
        public IEnumerable<User> getOffline()
        {
            return _rep.GetAllOffline();
        }

        [HttpGet]
        [ActionName("runingServer")]
        public string runingServer()
        {
            string baseUrl = Url.Request.RequestUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
            return $"The server is running on {baseUrl}";
        }

    }
}
