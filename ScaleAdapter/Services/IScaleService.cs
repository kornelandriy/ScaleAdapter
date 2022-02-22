using System.Threading.Tasks;
using ScaleAdapter.Models;

namespace ScaleAdapter.Services
{
    public interface IScaleService
    {
        Task<ScaleResponse> GetScaleValues(int scaleNo);
    }
}