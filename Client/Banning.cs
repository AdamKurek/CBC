using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace CBC.Client
{
    public static class Banning
    {
        public static async Task<DateTime?> GetBanEndDate(IJSRuntime js)
        {
            string endDateString = await js.InvokeAsync<string>("localStorage.getItem", "banEndDate");
            if (!string.IsNullOrEmpty(endDateString))
            {
                return DateTime.Parse(endDateString);
            }
            return null;
        }

        public static async Task SetBanEndDate(IJSRuntime js, DateTime endDate)
        {
            await js.InvokeVoidAsync("localStorage.setItem", "banEndDate", endDate.ToString());
        }
    }
}
