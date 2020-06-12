using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PoEW.API.Models;
using PoEW.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace PoEW.Data.Database {
    class SqliteContext : DbContext {
        public Mutex Lock { get; }

        public SqliteContext() {
            Lock = new Mutex();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options) {
            options.UseSqlite("Data Source=.\\data.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Item>()
                .Property(e => e.FlavourText)
                .HasConversion(v => string.Join(";", v), v => v.Split(';').ToList().FindAll(e => !string.IsNullOrEmpty(e)));
            modelBuilder.Entity<Item>()
                .Property(e => e.ExplicitMods)
                .HasConversion(v => string.Join(";", v), v => v.Split(';').ToList().FindAll(e => !string.IsNullOrEmpty(e)));
            modelBuilder.Entity<Item>()
                .Property(e => e.ImplicitMods)
                .HasConversion(v => string.Join(";", v), v => v.Split(';').ToList().FindAll(e => !string.IsNullOrEmpty(e)));
            modelBuilder.Entity<Item>()
                .Property(e => e.EnchantMods)
                .HasConversion(v => string.Join(";", v), v => v.Split(';').ToList().FindAll(e => !string.IsNullOrEmpty(e)));
            modelBuilder.Entity<Item>()
                .Property(e => e.Position)
                .HasConversion(v => $"{v.X};{v.Y}", v => new Position(v));
            modelBuilder.Entity<StashTab>()
                .Property(e => e.Colour)
                .HasConversion(v => $"{v.R};{v.G};{v.B}", v => new Color(v));
            modelBuilder.Entity<Item>()
              .Property(e => e.Properties)
              .HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<List<ItemProperty>>(v));
            modelBuilder.Entity<Item>()
             .Property(e => e.AddtionalProperties)
             .HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<List<ItemProperty>>(v));
            modelBuilder.Entity<Item>()
                .Property(e => e.Sockets)
                .HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<List<ItemSocket>>(v));
            modelBuilder.Entity<Item>()
               .Property(e => e.Requirements)
               .HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<List<ItemRequirement>>(v));
            modelBuilder.Entity<Item>()
              .Property(e => e.NextLevelRequirements)
              .HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<List<ItemRequirement>>(v));
            modelBuilder.Entity<Item>()
                .Property(e => e.SocketedItems)
                .HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<List<Item>>(v));
            modelBuilder.Entity<PoEW.API.Models.Price>()
                .Property(e => e.Value)
                .HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<Price>(v));
            modelBuilder.Entity<ShopThread>()
               .Property(e => e.League)
               .HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<League>(v));
        }

        public DbSet<Character> Characters { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<StashTab> StashTabs { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<PoEW.API.Models.Price> Prices { get; set; }
        public DbSet<ShopThread> Shops { get; set; }

    }
}
