using Google.Apis.Json;
using Google.Apis.Util.Store;
using GoogleCalendar_MVC.Data;
using GoogleCalendar_MVC.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCalendar_MVC.Configs
{
    /// <summary>
    /// File data store that implements <see cref="IDataStore"/>. This store creates a different file for each 
    /// combination of type and key. This file data store stores a JSON format of the specified object.
    /// </summary>
    public class MyDataStore : IDataStore
    {
        private static readonly Task CompletedTask = Task.FromResult(0);

        readonly ApplicationDbContext _db;

        public MyDataStore(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task StoreAsync<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            var serialized = NewtonsoftJsonSerializer.Instance.Serialize(value);
            var credential = GetCredentialFromDb(key, typeof(T));
            if (credential != null)
            {
                credential.JsonValue = serialized;
                _db.Credentials.Update(credential);
            }
            else
            {
                credential = new Credential()
                {
                    UserId = key,
                    Type = typeof(T).FullName,
                    JsonValue = serialized
                };
                _db.Credentials.Add(credential);
            }
            _db.SaveChanges();
            return CompletedTask;
        }


        public Task DeleteAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            var credential = GetCredentialFromDb(key, typeof(T));
            if (credential != null)
            {
                _db.Credentials.Remove(credential);
                _db.SaveChanges();
            }
            return CompletedTask;
        }


        public Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            var credential = GetCredentialFromDb(key, typeof(T));
            if (credential != null)
            {
                tcs.SetResult(NewtonsoftJsonSerializer.Instance.Deserialize<T>(credential.JsonValue));
            }
            else
            {
                tcs.SetResult(default(T));
            }
            return tcs.Task;
        }


        public Task ClearAsync()
        {
            var list = _db.Credentials.ToList();
            _db.Credentials.RemoveRange(list);

            return CompletedTask;
        }

        public Credential GetCredentialFromDb(string key, Type t)
        {
            return _db.Credentials.Where(c => c.UserId == key && c.Type == t.FullName).FirstOrDefault();
        }
    }
}
