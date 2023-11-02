using DisantAPI.Models;

namespace DisantAPI.Services.Interfaces
{
    public interface IRFIDService
    {
        Task<List<Workshop>> GetWorkersInOpenedWorkshops(long userId);
        Task<List<Custom>> GetWorkshops(long userId);
        Task<List<Custom>> GetWorkersByWorkshop(long userId, long workshopId);
        Task<int> PutOnBalance(long userId, List<string> epc);
        Task<int> PullFromBalance(long userId, List<string> epcs);
        Task<int> GiveEPC(long userId, List<string> epcs, long employeer, long workshop);
        Task<int> GetEPC(long userId, List<string> epcs);
        Task<bool> CheckPermission(long userId, int permission);
        Task<PermissionsApiResponse> GetPermissions(long userId);
        Task<List<RFIDBalance?>?> GetBalances();
        Task<List<RFIDJournal?>?> GetJournal();
        Task<List<RFIDInvoice?>?> GetInvoice();
        Task<List<RFIDBalance?>?> GetTagOnBalanceByWorkshop(long userID, long workshoID);
        Task<List<RFIDBalance?>?> GetAllTagsAtWorkshops(long userID);
    }
}
