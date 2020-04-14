using System.Threading.Tasks;

namespace Mews.LocalizationBuilder.Extensions
{
    public static class TaskExtensions
    {
        public static T Result<T>(this Task<T> task)
        {
            return Task.Run(() => task).Result;
        }
    }
}