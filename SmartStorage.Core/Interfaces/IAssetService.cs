using SmartStorage.Core.DTOs;
using SmartStorage.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStorage.Core.Interfaces
{
    public interface IAssetService
    {
        Task<Asset> CreateAsset(CreateAssetDto assetDto);
        Task<Bid> PlaceBid(PlaceBidDto bidDto);
        Task<IEnumerable<Asset>> GetAvailableAssets();
        Task<Asset> ProcessAuctionEnd(int assetId);
        Task SendPaymentReminders();
        Task SendFinalNotices();
    }
}
