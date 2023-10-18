using DBF.Models;
using DisantAPI.Models;
using DisantAPI.Repository;
using DisantAPI.Services.Classes;
using DisantAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Collections;
using System.Data;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DisantAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class DBRequestController : ControllerBase
    {
        private readonly ILogger<DBRequestController> _logger;
        private readonly DisantDBFRepository _repository;
        private readonly IRFIDService _rfidService;

        public DBRequestController(ILogger<DBRequestController> logger, DisantDBFRepository repository, IRFIDService rfidService)
        {
            _logger = logger;
            _repository = repository;
            _rfidService = rfidService;
        }
        
        [HttpPost]
        public async Task<string> Post([FromBody]string sqlRequest)
        {
            try
            {
                await _repository.Execute("set tablevalidate to 0");
                var data = await _repository.Execute(sqlRequest);
                if (data == null) return null;
                var result = JsonConvert.SerializeObject(data);
                return result;
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(ex);
            }
        }
        [Route("rfidpermissions")]
        [HttpGet]
        [ProducesResponseType(typeof(PermissionsApiResponse), 200)]
        public async Task<PermissionsApiResponse> GetPermissions(long userId)
        {
            return await _rfidService.GetPermissions(userId);
        }

        [Route("putonbalance")]
        [HttpPost]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<int> PutOnBalance(long userId, [FromBody] List<string> epc)
        {
            return await _rfidService.PutOnBalance(userId, epc);
        }

        [Route("pullfrombalance")]
        [HttpPost]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<int> PullFromBalance(long userId, [FromBody] List<string> epc)
        {
            return await _rfidService.PullFromBalance(userId, epc);
        }

        [Route("giveepc")]
        [HttpPost]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<int> GiveEPC(long userId, long employeer, long workshop, [FromBody] List<string> epc)
        {
            return await _rfidService.GiveEPC(userId, epc, employeer, workshop);
        }

        [Route("getepc")]
        [HttpPost]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<int> GetEPC(long userId, [FromBody] List<string> epc)
        {
            return await _rfidService.GetEPC(userId, epc);
        }

        [Route("rfidbalance")]
        [HttpGet]
        [ProducesResponseType(typeof(List<RFIDBalance>), 200)]
        public async Task<List<RFIDBalance?>?> GetBalances()
        {
            return await _rfidService.GetBalances();
        }

        [Route("rfidjournal")]
        [HttpGet]
        [ProducesResponseType(typeof(List<RFIDJournal>), 200)]
        public async Task<List<RFIDJournal?>?> GetJournal()
        {
            return await _rfidService.GetJournal();
        }

        [Route("rfidinvoice")]
        [HttpGet]
        [ProducesResponseType(typeof(List<RFIDInvoice>), 200)]
        public async Task<List<RFIDInvoice?>?> GetInvoice()
        {
            return await _rfidService.GetInvoice();
        }

        [Route("getworkersbyworkshop")]
        [HttpPost]
        [ProducesResponseType(typeof(List<Custom>), 200)]
        public async Task<List<Custom>> GetWorkersByWorkshop(long userId,long workshopId)
        {
            return await _rfidService.GetWorkersByWorkshop(userId, workshopId);
        }

        [Route("getopenedworkshops")]
        [HttpPost]
        [ProducesResponseType(typeof(List<Custom>), 200)]
        public async Task<List<Custom>> GetOpenedWorkshops(long userId)
        {
            return await _rfidService.GetWorkshops(userId);
        }

        [Route("getworkersinopenedworkshops")]
        [HttpPost]
        [ProducesResponseType(typeof(List<Workshop>), 200)]
        public async Task<List<Workshop>> GetWorkersInOpenedWorkshops(long userId)
        {
            return await _rfidService.GetWorkersInOpenedWorkshops(userId);
        }
    }
}