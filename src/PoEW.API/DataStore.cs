using Microsoft.EntityFrameworkCore;
using PoEW.Data.Abstractions;
using PoEW.Data.Database;
using PoEW.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PoEW.Data {
    public class DataStore : IDataStore {
        private readonly SqliteContext _context;
        public DataStore() {
            _context = new SqliteContext();

            Init();
        }

        private async void Init() {
            await _context.Database.EnsureCreatedAsync();
        }

        public async Task<List<T>> Get<T>(string[] includes = null) where T : Model {
            List<T> entities = new List<T>();

            _context.Lock.WaitOne();
            var query = _context.Set<T>().AsQueryable();

            if (includes != null) {
                foreach (var include in includes) {
                    query = query.Include(include);
                }
            }

            entities = await query.ToListAsync();
            _context.Lock.ReleaseMutex();

            return entities;
        }

        public async Task<List<T>> Get<T>(Predicate<T> predicate, string[] includes = null) where T : Model {
            var entities = (await Get<T>(includes)).FindAll(predicate);

            return entities;
        }

        public async Task<T> Get<T>(string id, string[] includes = null) where T : Model {
            var entity = (await Get<T>(includes)).Find(t => t.Id == id);

            return entity;
        }

        public async Task<T> Insert<T>(T entity) where T : Model {
            _context.Lock.WaitOne();
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            _context.Lock.ReleaseMutex();

            return entity;
        }

        public async Task<List<T>> Insert<T>(List<T> entities) where T : Model {
            foreach (var entity in entities) {
                await Insert(entity);
            }

            return entities;
        }

        public async Task<T> Update<T>(T entity, string[] includes = null) where T : Model {
            _context.Lock.WaitOne();
            _context.Entry(await Get<T>(entity.Id, includes)).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            _context.Lock.ReleaseMutex();

            return entity;
        }

        public async Task<T> Delete<T>(string id) where T : Model {
            var entity = await Get<T>(id);

            _context.Lock.WaitOne();
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            _context.Lock.ReleaseMutex();

            return entity;
        }

        public async Task<T> Save<T>(T entity, string[] includes = null) where T : Model {
            var existingEntity = await Get<T>(entity.Id);

            if (existingEntity == null) {
                await Insert(entity);
            } else {
                await Update(entity, includes);
            }

            return entity;
        }

        public async Task<List<T>> Save<T>(List<T> entities, string[] includes = null) where T : Model {
            foreach (var e in entities) {
                await Save(e, includes);
            }

            return entities;
        }

        public async Task<List<T>> DeleteAll<T>() where T : Model {
            var entities = await Get<T>();

            foreach (var e in entities) {
                await Delete<T>(e.Id);
            }

            return entities;
        }
    }
}
