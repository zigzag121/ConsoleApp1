using ConsoleApp1.Interfaces;
using ConsoleApp1.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Services
{
    public class DepthChartService
    {
        private readonly IDepthChartRepository _repository;
        private readonly ILogger<DepthChartService> _logger;

        public DepthChartService(IDepthChartRepository repository, ILogger<DepthChartService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public void AddPlayerToDepthChart(string position, Player player, int positionDepth = -1)
        {
            _logger.LogInformation("Adding player {PlayerName} to position {Position}", player.Name, position);

            // Check if the player number is unique
            var existingPlayerWithNumber = _repository.GetPlayerByNumber(player.Number);
            if (existingPlayerWithNumber != null && existingPlayerWithNumber.PlayerId != player.PlayerId)
            {
                _logger.LogWarning("Player number {PlayerNumber} is already taken", player.Number);
                throw new InvalidOperationException($"Player number {player.Number} is already taken.");
            }

            var existingEntries = _repository.GetDepthChartEntries(position).ToList();

            if (positionDepth < 0 || positionDepth >= existingEntries.Count)
            {
                positionDepth = existingEntries.Count;
            }
            else
            {
                foreach (var entry in existingEntries.Where(e => e.PositionDepth >= positionDepth))
                {
                    entry.PositionDepth++;
                }
            }

            var newEntry = new DepthChartEntry
            {
                Position = position,
                PositionDepth = positionDepth,
                Player = player,
                PlayerId = player.PlayerId
            };

            _repository.AddPlayer(player); // Ensure the player is added to the database
            _repository.AddDepthChartEntry(newEntry);
            _repository.SaveChanges();

            _logger.LogInformation("Player {PlayerName} added to position {Position} at depth {Depth}", player.Name, position, positionDepth);
        }

        public Player RemovePlayerFromDepthChart(string position, Player player)
        {
            _logger.LogInformation($"Removing player {player.Name} from position {position}");

            var entry = _repository.GetDepthChartEntry(position, player.PlayerId);
            if (entry == null)
            {
                _logger.LogWarning($"Player {player.Name} not found in position {position}");
                return null;
            }

            var entriesToAdjust = _repository.GetDepthChartEntries(position)
                                              .Where(e => e.PositionDepth > entry.PositionDepth)
                                              .OrderBy(e => e.PositionDepth)
                                              .ToList();

            _repository.RemoveDepthChartEntry(entry);

            foreach (var e in entriesToAdjust)
            {
                e.PositionDepth--;
            }

            _repository.SaveChanges();

            _logger.LogInformation($"Player {player.Name} removed from position {position}");

            return player;
        }


        public List<Player> GetBackups(string position, Player player)
        {
            var entry = _repository.GetDepthChartEntry(position, player.PlayerId);
            if (entry == null)
            {
                _logger.LogWarning($"Player {player.Name} not found in position {position}");
                return new List<Player>();
            }

            var backups = _repository.GetDepthChartEntries(position)
                                     .Where(e => e.PositionDepth > entry.PositionDepth)
                                     .OrderBy(e => e.PositionDepth)
                                     .Select(e => e.Player)
                                     .ToList();

            _logger.LogInformation($"Backups for player {player.Name} in position {position} retrieved");

            return backups;
        }

        public string GetFullDepthChart()
        {
            var depthChartEntries = _repository.GetDepthChartEntries(null).OrderBy(e => e.Position).ThenBy(e => e.PositionDepth);
            var positions = depthChartEntries.GroupBy(e => e.Position);

            var sb = new StringBuilder();
            foreach (var position in positions)
            {
                sb.Append($"{position.Key} – ");
                sb.Append(string.Join(", ", position.Select(e => $"#{e.Player.Number}, {e.Player.Name}")));
                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }
    }
}
