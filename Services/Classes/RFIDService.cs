using DisantAPI.Models;
using DisantAPI.Repository;
using DisantAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.OpenApi.Validations;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Security.Policy;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DisantAPI.Services.Classes
{
    public class RFIDService : IRFIDService
    {
        private readonly DisantDBFRepository _repository;

        public RFIDService(DisantDBFRepository repository)
        {
            _repository = repository;
        }

        public void CheckLog(string message)
        {
            
        }

        public async Task<List<Workshop>> GetWorkersInOpenedWorkshops(long userId)
        {
            //var action = (int)PermissionEnum.AcceptanceFromSupplier;
            //if (!await CheckPermission(userId, action)) { return -2; }
            return await GetCustomsInOpenedWorkshops();
        }

        public async Task<List<Custom>> GetWorkersByWorkshop(long userId, long workshopId)
        {
            //var action = (int)PermissionEnum.AcceptanceFromSupplier;
            //if (!await CheckPermission(userId, action)) { return -2; }
            if (await CheckIsWorkshop(workshopId))
                return await GetCustomsByWorkshop(workshopId);
            return new List<Custom>();
        } 

        public async Task<List<Custom>> GetWorkshops(long userId)
        {
            //var action = (int)PermissionEnum.AcceptanceFromSupplier;
            //if (!await CheckPermission(userId, action)) { return -2; }
            return await GetOpenedWorkShops();
        }

        public async Task<int> PutOnBalance(long userId, List<string> epcs)
        {
            var action = (int)PermissionEnum.AcceptanceFromSupplier;
            return await ManageBalance(userId, epcs, action);
        }

        public async Task<int> PullFromBalance(long userId, List<string> epcs)
        {
            var action = (int)PermissionEnum.TransferToSupplier;
            return await ManageBalance(userId, epcs, action);
        }

        public async Task<int> GetEPC(long userId, List<string> epcs)
        {
            var action = (int)PermissionEnum.AcceptanceFromWorker;
            return await ManageBalance(userId, epcs, action);
        }

        public async Task<int> GiveEPC(long userId, List<string> epcs, long employeer, long workshop)
        {
            var action = (int)PermissionEnum.TransferToWorker;
            return await ManageBalance(userId, epcs, action, employeer, workshop);
        }

        private async Task<int> ManageBalance(long userId, List<string> epcs, int action, long employee = 0, long workshop = 0)
        {
            if (!epcs.Any()) { return -1; }
            //if (!await CheckPermission(userId, action)) { return -2; }
            var listB = new List<RFIDBalance>();
            var onBalance = false;
            if (action == (int)PermissionEnum.AcceptanceFromSupplier || action == (int)PermissionEnum.TransferToWorker || action == (int)PermissionEnum.AcceptanceFromWorker)
            {
                onBalance = true;
            }

            foreach (var epc in epcs)
            {
                listB.Add(new RFIDBalance() { Employee = employee, Tag = epc.Trim(), OnBalance = onBalance, Workshop = workshop });
            }

            try
            {
                if (!await AddOrInsertBalance(listB)) { return -1; }
                var journal = new RFIDJournal() { Action = action, Number = await GetNextRFIDJournalKey(), Workshop = workshop, CUser = userId};
                if (await AddJournal(journal))
                {
                    var listI = new List<RFIDInvoice>();
                    foreach (var balance in listB)
                    {
                        listI.Add(new RFIDInvoice() { Tag = balance.Tag, Employee = balance.Employee, Journal = journal.Number });
                    }
                    await AddInvoices(listI);
                }
            }
            catch (Exception ex)
            {

            }
            return 1;
        }

        public async Task<bool> CheckPermission(long userId, int permission)
        {
            var query = "SELECT DISTINCT Permission " +
                "FROM Rfidpermissions " +
                $"WHERE Employee == {userId} AND Permission == {permission}";
            var obj = await GetObjectsFromQueryAsync<List<RFIDPermission>>(query);
            if (obj != default && obj.Any())
                return true;
            else
                return false;
        }
        public async Task<PermissionsApiResponse?> GetPermissions(long userId)
        {
            var query = "SELECT number, name " +
                "FROM custom " +
                $"WHERE number == {userId}";
            var customs = await GetObjectsFromQueryAsync<List<Custom>>(query);
            if (customs == default)
            {
                return default;
            }

            var custom = customs.FirstOrDefault();
            if (custom == null)
            {
                return default;
            }

            query = "SELECT DISTINCT Rfidpermissions.Permission " +
                "FROM Rfidpermissions " +
                $"WHERE Rfidpermissions.Employee == {userId}";
            var obj = await GetObjectsFromQueryAsync<List<RFIDPermission>>(query);
            if (obj == default)
            {
                return default;
            }
            else
            {
                return new PermissionsApiResponse()
                {
                    UserId = userId,
                    Name = custom.Name,
                    Permissions = obj
                };
            }
        }

        private async Task<List<Custom>> GetCustomsByWorkshop(long workshopId)
        {
            var query = "SELECT custom.* " +
                "FROM worktime as wt " +
                $"INNER JOIN (SELECT MAX(number) as number FROM worktime WHERE idn == {workshopId} AND EMPTY(timeend) AND timebeg > datetime() - 24 * 60 * 60) as workshop ON wt.parent == workshop.number " +
                "INNER JOIN custom ON custom.number == wt.idn";
            var obj = await GetObjectsFromQueryAsync<List<Custom>>(query);
            if (obj == default || obj.Count <= 0)
            {
                return new List<Custom>();
            }
            else
            {
                return obj;
            }
        }
                

        private async Task<List<Custom>> GetOpenedWorkShops()
        {
            var query = "SELECT DISTINCT custom.* " +
                "FROM custom " +
                "INNER JOIN worktime ON worktime.idn == custom.number " +
                "WHERE EMPTY(worktime.timeend) " +
                    "AND worktime.timebeg > datetime() - 24 * 60 * 60 " +
                    "AND 'Ц'$custom.Mark";
            var obj = await GetObjectsFromQueryAsync<List<Custom>>(query);
            if (obj != default && obj.Any() && obj.Count > 0)
                return obj.Where(c => c.Mark.Contains("Ц")).ToList();
            return new List<Custom>();
        }

        private async Task<List<Workshop>> GetCustomsInOpenedWorkshops()
        {
            var query = "SELECT worker.number, worker.name, worker.mark, workshop.number as workshopidn, workshop.name as workshopname " +
                "FROM custom as worker " +
                "INNER JOIN worktime as wtw ON wtw.idn == worker.number " +
                "INNER JOIN worktime as wtws ON wtw.parent == wtws.number " +
                "INNER JOIN custom as workshop ON workshop.number == wtws.idn " +
                "WHERE EMPTY(wtws.timeend) " +
                    "AND wtws.timebeg > datetime() - 24 * 60 * 60 " +
                    "AND UPPER(CHR(246))$UPPER(workshop.Mark) " +
                "ORDER BY workshopidn";

            var obj = await GetObjectsFromQueryAsync<List<WorkersInContextOfWorkshops>>(query);
            var workshops = new List<Workshop>();
            if (obj != default && obj.Any() && obj.Count > 0)
            {
                var filteredForWorkshops = obj.GroupBy(ws => ws.WorkshopId).ToList().Select(ws => ws.First()).ToList();
                foreach(var ws in filteredForWorkshops)
                {
                    var workers = new List<Custom>();
                    var filteredForWorkers = obj.Where(w => w.WorkshopId == ws.WorkshopId).ToList();
                    foreach(var worker in filteredForWorkers)
                    {
                        workers.Add(new Custom() { DisanId = worker.DisanId, Name = worker.Name, Mark = worker.Mark });
                    }
                    workshops.Add(new Workshop() { DisanId = ws.WorkshopId, Name = ws.WorkshopName, Workers = workers });
                }

            }
            return workshops;
        }

        public async Task<List<RFIDBalance?>?> GetTagOnBalanceByWorkshop(long userID, long workshopID)
        {
            var query = "SELECT * " +
                "FROM RFIDBalance " +
                $"WHERE workshop == {workshopID}";

            return await GetObjectsFromQueryAsync<List<RFIDBalance?>?>(query);
        }

        public async Task<List<RFIDBalance?>?> GetAllTagsAtWorkshops(long userID)
        {
            var query = "SELECT * " +
                "FROM RFIDBalance " +
                "WHERE NOT EMPTY(workshop) AND workshop <> 0";

            return await GetObjectsFromQueryAsync<List<RFIDBalance?>?>(query);
        }

        private async Task<long> GetNextRFIDJournalKey()
        {
            var query = "SELECT MAX(Number) as Number " +
                "FROM RFIDJournal";
            var obj = await GetObjectsFromQueryAsync<List<RFIDJournal>>(query);
            if (obj == default || !obj.Any()) 
            {
                return 101;
            }
            else
            {
                return obj.First().Number + 100;
            }
        }

        private async Task<bool> AddJournal(RFIDJournal journal)
        {
            try
            {
                var query = "INSERT INTO rfidjournal " +
                $"VALUES ({journal.Number}, {journal.Action}, DATETIME(), {journal.CUser}, {journal.Workshop})";
                var obj = await ExecuteQuery(query);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<bool> AddInvoices(List<RFIDInvoice> invoices)
        {
            foreach (var invoice in invoices)
            {
                bool result;
                result = await AddInvoice(invoice);
            }
            return true;
        }

        private async Task<bool> AddInvoice(RFIDInvoice invoice)
        {
            try
            {
                var query = "INSERT INTO rfidinvoice " +
                "(Journal, Tag, Employee) " +
                $"VALUES ({invoice.Journal}, '{invoice.Tag}', {invoice.Employee}) ";
                var obj = await ExecuteQuery(query);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<bool> AddOrInsertBalance(List<RFIDBalance> balances)
        {
            foreach (var balance in balances)
            {
                bool result;
                if (await CheckBalance(balance.Tag))
                {
                    result = await UpdateBalance(balance);
                }
                else
                {
                    result = await AddBalance(balance);
                }
            }
            return true;
        }

        public async Task<List<RFIDBalance?>?> GetBalances()
        {
            var query = "SELECT Tag, Onbalance, Employee, Workshop " +
               "FROM rfidbalance";
            return await GetObjectsFromQueryAsync<List<RFIDBalance?>?>(query);
        }

        public async Task<List<RFIDJournal?>?> GetJournal()
        {
            var query = "SELECT Number, Action, Modify, Cuser, Workshop " +
               "FROM rfidjournal";
            return await GetObjectsFromQueryAsync<List<RFIDJournal?>?>(query);
        }

        public async Task<List<RFIDInvoice?>?> GetInvoice()
        {
            var query = "SELECT Tag, Employee, Journal " +
               "FROM rfidinvoice";
            return await GetObjectsFromQueryAsync<List<RFIDInvoice?>?>(query);
        }

        private async Task<bool> CheckBalance(string epc)
        {
            var query = "SELECT Tag, Onbalance, Employee " +
                "FROM rfidbalance " +
                $"WHERE Tag == '{epc}'";
            var obj = await GetObjectsFromQueryAsync<List<RFIDBalance>>(query);
            if (obj != default && obj.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> CheckIsWorkshop(long workshopIdn)
        {
            var query = "SELECT Mark " +
                "FROM custom " +
                $"WHERE number  == {workshopIdn}";
            var obj = await GetObjectsFromQueryAsync<List<Custom>>(query);
            if (obj != default && obj.Any())
                if (obj.Where(c => c.Mark.Contains("Ц")).ToList().Count() > 0)
                    return true;
            return false;
        }

        private async Task<bool> AddBalance(RFIDBalance balance)
        {
            try
            {
                var query = "INSERT INTO rfidbalance " +
                $"VALUES ('{balance.Tag}', {balance.GetOnBalanceStringValue()}, {balance.Employee}, {balance.Workshop}) ";
                var obj = await ExecuteQuery(query);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<bool> UpdateBalance(RFIDBalance balance)
        {
            try
            {
                var query = "UPDATE rfidbalance " +
                $"SET Onbalance = {balance.GetOnBalanceStringValue()}, Employee = {balance.Employee}, Workshop = {balance.Workshop} " +
                $"WHERE Tag == '{balance.Tag}'";
                var obj = await ExecuteQuery(query);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<T?> GetObjectsFromQueryAsync<T>(string query)
        {
            if (!string.IsNullOrEmpty(query))
            {
                try
                {
                    var result = await ExecuteQuery(query);
                    if (!string.IsNullOrEmpty(result))
                    {
                        return GetObjectFromDataString<T>(result);
                    }
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                    var stacktrace = ex.StackTrace;
                    return default(T?);
                }
            }
            return default(T?);
        }

        private T? GetObjectFromDataString<T>(string dataString)
        {
            if (typeof(T) == typeof(List<RFIDJournal>)
                || typeof(T) == typeof(List<RFIDInvoice>)
                || typeof(T) == typeof(List<RFIDPermission>)
                || typeof(T) == typeof(Custom))
                dataString = dataString.Replace(".0", string.Empty);
            
            return JsonConvert.DeserializeObject<T>(dataString);
        }

        private async Task<string> ExecuteQuery(string sqlRequest)
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
                throw;
            }
        }
    }
}
