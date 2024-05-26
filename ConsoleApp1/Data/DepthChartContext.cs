using ConsoleApp1.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Data
{
    public class DepthChartContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<DepthChartEntry> DepthChartEntries { get; set; }

        public DepthChartContext(DbContextOptions<DepthChartContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(e => e.PlayerId);
                entity.Property(e => e.Name).HasColumnType("TEXT"); 
                entity.HasIndex(e => e.Number).IsUnique();
            });

            modelBuilder.Entity<DepthChartEntry>(entity =>
            {
                entity.HasOne(d => d.Player)
                      .WithMany()
                      .HasForeignKey(d => d.PlayerId);
            });
        }
    }
}
