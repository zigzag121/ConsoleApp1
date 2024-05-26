using ConsoleApp1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Interfaces
{
    public interface IDepthChartRepository
    {
        void AddPlayer(IPlayer player);
        void AddDepthChartEntry(DepthChartEntry entry);
        IPlayer GetPlayer(int playerId);
        DepthChartEntry GetDepthChartEntry(string position, int playerId);
        IEnumerable<DepthChartEntry> GetDepthChartEntries(string position);
        IPlayer GetPlayerByNumber(int number);
        void RemoveDepthChartEntry(DepthChartEntry entry);
        void RemoveAllPlayersAndEntries();
        void SaveChanges();
    }
}
