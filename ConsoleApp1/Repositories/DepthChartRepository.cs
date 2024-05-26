using ConsoleApp1.Data;
using ConsoleApp1.Interfaces;
using ConsoleApp1.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Repositories
{
    public class DepthChartRepository : IDepthChartRepository
    {
        private readonly DepthChartContext _context;

        public DepthChartRepository(DepthChartContext context)
        {
            _context = context;
        }

        public void AddPlayer(IPlayer player)
        {
            // Only add if the player does not already exist
            if (_context.Players.Find(player.PlayerId) == null)
            {
                _context.Players.Add((Player)player);
            }
        }

        public void AddDepthChartEntry(DepthChartEntry entry)
        {
            _context.DepthChartEntries.Add(entry);
        }

        public IPlayer GetPlayer(int playerId)
        {
            return _context.Players.Find(playerId);
        }

        public DepthChartEntry GetDepthChartEntry(string position, int playerId)
        {
            return _context.DepthChartEntries
                .FirstOrDefault(e => e.Position == position && e.PlayerId == playerId);
        }

        public IEnumerable<DepthChartEntry> GetDepthChartEntries(string position)
        {
            if (position != null)
            {
                return _context.DepthChartEntries.Include(e => e.Player).Where(e => e.Position == position).OrderBy(e => e.PositionDepth).ToList();
            }

            return _context.DepthChartEntries.Include(e => e.Player).OrderBy(e => e.PositionDepth).ToList();
        }

        public IPlayer GetPlayerByNumber(int number)
        {
            return _context.Players.FirstOrDefault(p => p.Number == number);
        }

        public void RemoveDepthChartEntry(DepthChartEntry entry)
        {
            _context.DepthChartEntries.Remove(entry);
        }
        public void RemoveAllPlayersAndEntries()
        {
            _context.DepthChartEntries.RemoveRange(_context.DepthChartEntries);
            _context.Players.RemoveRange(_context.Players);
            _context.SaveChanges();
        }


        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
