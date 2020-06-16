using PoEW.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PoEW.Data.Abstractions {
    public interface IDataStore {
        Task<List<T>> Get<T>(string[] includes = null) where T : Model;
        Task<List<T>> Get<T>(Predicate<T> predicate, string[] includes = null) where T : Model;
        Task<T> Get<T>(string id, string[] includes = null) where T : Model;
        Task<T> Insert<T>(T entity) where T : Model;
        Task<List<T>> Insert<T>(List<T> entities) where T : Model;
        Task<T> Update<T>(T entity, string[] includes = null) where T : Model;
        Task<T> Delete<T>(string id) where T : Model;
        Task<List<T>> DeleteAll<T>(Predicate<T> predicate) where T : Model;
        Task<T> Save<T>(T entity, string[] includes = null) where T : Model;
        Task<List<T>> Save<T>(List<T> entities, string[] includes = null) where T : Model;
    }
}
