using SmartStorage.Core.DTOs;
using SmartStorage.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStorage.Core.Interfaces
{
    public interface ICartageService
    {
        Task<Cartage> CreateCartage(CreateCartageDto cartageDto);
        Task<Cartage> AssignDriver(int cartageId, int driverId);
        Task<Cartage> UpdateCartageStatus(int cartageId, string status);
        Task<IEnumerable<Cartage>> GetDriverCartages(int driverId);
    }
}
