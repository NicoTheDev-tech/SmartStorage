using SmartStorage.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartStorage.Core.Interfaces
{
    public interface IContractService
    {
        Task<ContractResponseDto> CreateContract(CreateContractDto createDto, string adminId);
        Task<ContractResponseDto?> GetContractById(int id);
        Task<ContractResponseDto> AcceptContract(AcceptContractDto acceptDto, string userId, string ipAddress);
        Task<ContractResponseDto> ActivateContract(int contractId, string adminId);
        Task<IEnumerable<ContractResponseDto>> GetClientContracts(string userId);
        Task<IEnumerable<ContractResponseDto>> GetAllContracts();
        Task<ContractResponseDto> GenerateContractFromBooking(int bookingId, string adminId);
        Task<bool> ContractExistsForBooking(int bookingId);
    }
}