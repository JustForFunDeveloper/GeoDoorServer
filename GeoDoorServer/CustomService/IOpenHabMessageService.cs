using System.Threading.Tasks;

namespace GeoDoorServer.CustomService
{
    public interface IOpenHabMessageService
    {
        Task<string> GetData(string itemName);
        Task PostData(string itemName, string value, bool toggle = false);
    }
}
