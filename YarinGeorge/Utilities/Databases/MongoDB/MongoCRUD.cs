using EasyConsole;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Linq;
using MongoSync;
using Serilog;
using Serilog.Formatting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Extra;
using BsonDocument = MongoDB.Bson.BsonDocument;

namespace YarinGeorge.Utilities.Databases.MongoDB
{
    #region Connection Options Settings
    /// <summary>
    /// Connection options for your database. Used for when creating a new <see cref="MongoCRUD"/> connection.
    /// </summary>
    public class MongoCRUDConnectionOptions
    {
        /// <summary>
        /// The name of the database you want to access.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// The ConnectionString given to you by the MongoDB Atlas (Or any other service) database dashboard.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Auto reconnect on connection failure. Defaults to true because connection failures are annoying :)
        /// </summary>
        public bool AutoReconnect { get; set; } = true;

        public MongoDatabaseSettings MongoDbDatabaseSettings { get; set; }
    }

    #endregion

    /// <summary>
    /// A class for accessing MongoDB data with support for targeting both synchronous and asynchronous (async) code environments. 
    /// </summary>
    public class MongoCRUD
    {
        #region Connect
        private IMongoDatabase db { get; set; }
        public IMongoClient MongoClient { get; private set; }
        private static bool WasChanged { get; set; }

        /// <summary>
        /// Initializes a new MongoDB CRUD database connection.
        /// </summary>
        /// <param name="connectionOptions">The <see cref="MongoCRUDConnectionOptions"/> for this connection.</param>
        public MongoCRUD(MongoCRUDConnectionOptions connectionOptions, MongoDatabaseSettings databaseSettings = null)
        {
            int Tries = 0;
        Connect:
            try
            {
                Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                //.WriteTo.EventLog(typeof(MongoCRUD).Assembly.FullName)
                .CreateLogger();

                Log.Logger.Information($"Connecting to '{connectionOptions.Database}' Mongo database ...");
                MongoClient = new MongoClient(connectionOptions.ConnectionString);

                db = MongoClient.GetDatabase(connectionOptions.Database, databaseSettings);
            }
            catch (Exception exception)
            {
                if (Tries <= 5)
                {
                    Tries++;
                    Output.WriteLine(ConsoleColor.Red,
                        $"An errored while trying to connect to the {connectionOptions.Database} Mongo database. [This is attempt number {Tries}/5, Waiting 5 seconds before reconnecting].");
                    Task.Delay(5 * 1000);
                    goto Connect;
                }
                else
                {
                    DebuggingUtils.OutputBigError(
                        "Too many failed attempts... Press any key to close the program and maybe try again. If the problem consists, consider checking you connection string or 'allowed to access' IPs to see if you are not being blocked.");
                    Console.ReadKey();
                    Environment.Exit(69);
                }
            }
        }

        /// <summary>
        /// Initializes a new MongoDB CRUD database connection.
        /// </summary>
        /// <param name="MongoDBConnectionString">The <see cref="ConnectionString"/> for this connection.</param>
        public MongoCRUD(ConnectionString MongoDBConnectionString, MongoDatabaseSettings databaseSettings = null)
        {
            int Tries = 0;
        Connect:
            try
            {
                Output.WriteLine(ConsoleColor.Magenta,
                    $"Connecting to '{MongoDBConnectionString.DatabaseName}' Mongo database ...");
                MongoClient = new MongoClient(MongoDBConnectionString.ToString());

                db = MongoClient.GetDatabase(MongoDBConnectionString.DatabaseName, databaseSettings);
            }
            catch (Exception exception)
            {
                if (Tries <= 5)
                {
                    Tries++;
                    Output.WriteLine(ConsoleColor.Red,
                        $"An errored while trying to connect to the {MongoDBConnectionString.DatabaseName} Mongo database. [This is attempt number {Tries}/5, Waiting 5 seconds before reconnecting].");
                    Task.Delay(5 * 1000);
                    goto Connect;
                }
                else
                {
                    DebuggingUtils.OutputBigError(
                        "Too many failed attempts... Press any key to close the program and maybe try again. If the problem consists, consider checking you connection string or 'allowed to access' IPs to see if you are not being blocked.");
                    Console.ReadKey();
                    Environment.Exit(69);
                }
            }
        }

        /// <summary>
        /// Initializes a new MongoDB CRUD database connection.
        /// </summary>
        /// <param name="MongoDBConnectionStringURL">The <see cref="MongoUrl"/> for this connection.</param>
        public MongoCRUD(MongoUrl MongoDBConnectionStringURL, MongoDatabaseSettings databaseSettings = null)
        {
            int Tries = 0;
        Connect:
            try
            {
                Output.WriteLine(ConsoleColor.Magenta,
                    $"Connecting to '{MongoDBConnectionStringURL.DatabaseName}' Mongo database ...");
                MongoClient = new MongoClient(MongoDBConnectionStringURL);

                db = MongoClient.GetDatabase(MongoDBConnectionStringURL.DatabaseName, databaseSettings);
            }
            catch (Exception exception)
            {
                if (Tries <= 5)
                {
                    Tries++;
                    Output.WriteLine(ConsoleColor.Red,
                        $"An errored while trying to connect to the {MongoDBConnectionStringURL.DatabaseName} Mongo database. [This is attempt number {Tries}/5, Waiting 5 seconds before reconnecting].");
                    Task.Delay(5 * 1000);
                    goto Connect;
                }
                else
                {
                    DebuggingUtils.OutputBigError(
                        "Too many failed attempts... Press any key to close the program and maybe try again. If the problem consists, consider checking you connection string or 'allowed to access' IPs to see if you are not being blocked.");
                    Console.ReadKey();
                    Environment.Exit(69);
                }
            }
        }

        #endregion

        #region Get Collection

        public async Task<IMongoCollection<T>> GetCollectionAsync<T>(string CollectionName)
        {
            int Tries = 0;
        GetCollection:
            try
            {
                var collection = db.GetCollection<T>(CollectionName);
                return collection;
            }
            catch
            {
                if (Tries <= 5)
                {
                    Tries++;
                    Output.WriteLine(ConsoleColor.Red,
                        $"An errored while trying to connect to the {CollectionName} Mongo database. [This is attempt number {Tries}/5, Waiting 5 seconds before reconnecting].");
                    await Task.Delay(5 * 1000);
                    goto GetCollection;
                }
                else
                {
                    DebuggingUtils.OutputBigError(
                        "Too many failed attempts... Press any key to close the program and maybe try again. If the problem consists, consider checking you connection string or 'allowed to access' IPs to see if you are not being blocked.");
                    Console.ReadKey();
                    Environment.Exit(69);
                }

                return null;
            }
        }

        #endregion

        #region Insert/Upsert Records

        /// <summary>
        /// Inserts a single new schema (record) to a given collection.
        /// </summary>
        /// <param name="table">The collection to insert the schema to.</param>
        /// <param name="record">The record (AKA the schema) to insert to the collection.</param>
        /// <typeparam name="T">The schema model/schema structure (E.g a POCO class) to insert.</typeparam>
        public void InsertOneRecord<T>(string table, T record)
        {
            try
            {
                var collection = db.GetCollection<T>(table);
                collection.InsertOne(record);
            }
            catch (MongoWriteException MWE)
            {
                MWE.OutputBigExceptionError();
                //DebuggingUtils.OutputBigError($"{MWE.Message} : {MWE.InnerException}");
            }
        }

        public void InsertOneRecord<T>(IMongoCollection<T> collection, T record)
        {
            try
            {
                collection.InsertOne(record);
            }
            catch (MongoWriteException MWE)
            {
                MWE.OutputBigExceptionError();
                //DebuggingUtils.OutputBigError($"{MWE.Message} : {MWE.InnerException}");
            }
        }

        /// <summary>
        /// Asynchronously inserts a single new schema (record) to a given collection.
        /// </summary>
        /// <param name="table">The collection to insert the schema to.</param>
        /// <param name="record">The record (AKA the schema) to insert to the collection.</param>
        /// <typeparam name="T">The schema model/schema structure (E.g a POCO class) to insert.</typeparam>
        public async Task<T> InsertOneRecordAsync<T>(string table, T record)
        {
            try
            {
                var collection = db.GetCollection<T>(table);
                await collection.InsertOneAsync(record);
            }
            catch (MongoWriteException MWE)
            {
                MWE.OutputBigExceptionError();
                //DebuggingUtils.OutputBigError($"{MWE.Message} : {MWE.InnerException}");
            }
            return record;
        }

        public async Task<T> InsertOneRecordAsync<T>(IMongoCollection<T> collection, T record)
        {
            try
            {
                await collection.InsertOneAsync(record);
            }
            catch (MongoWriteException MWE)
            {
                MWE.OutputBigExceptionError();
                //DebuggingUtils.OutputBigError($"{MWE.Message} : {MWE.InnerException}");
            }
            return record;
        }

        public async Task<BsonDocument> InsertOneRecordAsync<T>(string table, BsonDocument record)
        {
            try
            {
                var collection = db.GetCollection<T>(table);
                var ConvertedRecord = BsonDocumentToType<T>(collection, record);
                await collection.InsertOneAsync(ConvertedRecord);
            }
            catch (MongoWriteException MWE)
            {
                MWE.OutputBigExceptionError();
                //DebuggingUtils.OutputBigError($"{MWE.Message} : {MWE.InnerException}");
            }
            return record;
        }

        public async Task<BsonDocument> InsertOneRecordAsync<T>(IMongoCollection<T> collection, BsonDocument record)
        {
            try
            {
                var ConvertedRecord = BsonDocumentToType<T>(collection, record);
                await collection.InsertOneAsync(ConvertedRecord);
            }
            catch (MongoWriteException MWE)
            {
                MWE.OutputBigExceptionError();
                //DebuggingUtils.OutputBigError($"{MWE.Message} : {MWE.InnerException}");
            }
            return record;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="Docs"></param>
        /// <returns></returns>
        public async Task InsertManyAsync<T>(string table, List<T> Docs)
        {
            var collection = db.GetCollection<T>(table);
            await collection.InsertManyAsync(Docs);
        }

        public async Task InsertManyAsync<T>(IMongoCollection<T> collection, List<T> Docs)
        {
            await collection.InsertManyAsync(Docs);
        }

        public async Task<ReplaceOneResult> UpsertAsync<T>(string table, string id, T BackupDoc)
        {
            var collection = db.GetCollection<T>(table);
            var Update = await collection.ReplaceOneAsync(new BsonDocument("_id", id), BackupDoc,
                new ReplaceOptions { IsUpsert = true });

            return Update;
        }

        public async Task<ReplaceOneResult> UpsertAsync<T>(string table, ulong id, T BackupDoc)
        {
            var collection = db.GetCollection<T>(table);
            var Update = await collection.ReplaceOneAsync(new BsonDocument("_id", long.Parse(id.ToString())), BackupDoc,
                new ReplaceOptions { IsUpsert = true });

            return Update;
        }

        public async Task<ReplaceOneResult> UpsertAsync<T>(string table, long id, T BackupDoc)
        {
            var collection = db.GetCollection<T>(table);
            var Update = await collection.ReplaceOneAsync(new BsonDocument("_id", id), BackupDoc,
                new ReplaceOptions { IsUpsert = true });

            return Update;
        }

        public async Task<ReplaceOneResult> UpsertAsync<T>(string table, int id, T BackupDoc)
        {
            var collection = db.GetCollection<T>(table);
            var Update = await collection.ReplaceOneAsync(new BsonDocument("_id", id), BackupDoc,
                new ReplaceOptions { IsUpsert = true });

            return Update;
        }

        public async Task<ReplaceOneResult> UpsertAsync<T>(string table, Guid id, T BackupDoc)
        {
            var collection = db.GetCollection<T>(table);
            var Update = await collection.ReplaceOneAsync(new BsonDocument("_id", id), BackupDoc,
                new ReplaceOptions { IsUpsert = true });

            return Update;
        }

        public async Task<ReplaceOneResult> UpsertAsync<T>(IMongoCollection<T> collection, string id, T BackupDoc)
        {
            Console.WriteLine(id);
            var Update = await collection.ReplaceOneAsync(new BsonDocument("_id", id), BackupDoc,
                new ReplaceOptions { IsUpsert = true });

            return Update;
        }

        #endregion

        #region Load Records or specific values
        /// <summary>
        /// Loads all records from a given collection (table).
        /// </summary>
        /// <param name="table">The collection to load all the documents from.</param>
        /// <typeparam name="T">The schema model/schema structure (E.g a POCO class) to load the records as.</typeparam>
        /// <returns>All the records from the given collection (table) as a <see cref="List{T}"/>.</returns>
        public List<T> LoadAllRecords<T>(string table)
        {
            var collection = db.GetCollection<T>(table);
            return collection.Find(new BsonDocument()).ToList();
        }

        public List<T> LoadAllRecords<T>(IMongoCollection<T> collection)
        {
            return collection.Find(new BsonDocument()).ToList();
        }

        /// <summary>
        /// Asynchronously loads all records from a given collection (table).
        /// </summary>
        /// <param name="table">The collection to load all the documents from.</param>
        /// <typeparam name="T">The schema model/schema structure (E.g a POCO class) to load the records as.</typeparam>
        /// <returns>All the records from the given collection (table) as a <see cref="List{T}"/>.</returns>
        public async Task<List<T>> LoadAllRecordsAsync<T>(string table)
        {
            var collection = db.GetCollection<T>(table);
            return await collection.FindAsync(new BsonDocument()).ConfigureAwait(false).GetAwaiter().GetResult()
                .ToListAsync();
        }

        public async Task<List<T>> LoadAllRecordsAsync<T>(IMongoCollection<T> collection)
        {
            return await collection.FindAsync(new BsonDocument()).ConfigureAwait(false).GetAwaiter().GetResult()
                .ToListAsync();
        }

        /// <summary>
        /// Load all specified field values by specifying the field name.
        /// </summary>
        /// <param name="table">The collection to load all the fields values from.</param>
        /// <param name="Field">The name of the field.</param>
        /// <typeparam name="T">The schema model/schema structure (E.g a POCO class) to load the records fields values as.</typeparam>
        /// <returns>All the fields values of records from the given collection (table) as a <see cref="List{T}"/></returns>
        public List<T> LoadRecsFieldsValuesByField<T>(string table, string Field)
        {
            var collection = db.GetCollection<T>(table);
            return collection.Find(Field).ToList();
        }

        public List<T> LoadRecsFieldsValuesByField<T>(IMongoCollection<T> collection, string Field)
        {
            return collection.Find(Field).ToList();
        }

        public async Task<List<T>> LoadManyByFieldsValuesAndFieldsAsync<T>(IMongoCollection<T> collection, string Field, List<string> Values)
        {
            List<T> Records = new List<T>();

            foreach (var Value in Values)
            {
                var Filter = Builders<T>.Filter.Eq(Field, Value);
                var results = (await collection.FindAsync(Filter)).First();
                Records.Add(results);
            }

            return Records;
        }

        public async Task<List<T>> LoadManyByFieldsValuesAndFieldsAsync<T>(IMongoCollection<T> collection, string Field, List<ulong> Values)
        {
            List<T> Records = new List<T>();

            foreach (var Value in Values)
            {
                var Filter = Builders<T>.Filter.Eq(Field, Value);
                var results = (await collection.FindAsync(Filter)).First();
                Records.Add(results);
            }

            return Records;
        }

        public async Task<List<T>> LoadManyByFieldsValuesAndFieldsAsync<T>(string table, string Field, List<string> Values)
        {
            var collection = db.GetCollection<T>(table);

            List<T> Records = new List<T>();

            foreach (var Value in Values)
            {
                var Filter = Builders<T>.Filter.Eq(Field, Value);
                var results = (await collection.FindAsync(Filter)).First();
                Records.Add(results);
            }

            return Records;
        }

        public async Task<List<T>> LoadManyByFieldsValuesAndFieldsAsync<T>(string table, string Field, List<ulong> Values)
        {
            var collection = db.GetCollection<T>(table);

            List<T> Records = new List<T>();

            foreach (var Value in Values)
            {
                var Filter = Builders<T>.Filter.Eq(Field, Value);
                var results = (await collection.FindAsync(Filter)).First();
                Records.Add(results);
            }

            return Records;
        }

        public async Task<List<T>> LoadManyByFieldsValuesAndFieldsAsync<T>(string table, string Field, IEnumerable<ulong> Values)
        {
            var collection = db.GetCollection<T>(table);

            List<T> Records = new List<T>();

            foreach (var Value in Values)
            {
                var Filter = Builders<T>.Filter.Eq(Field, Value);
                var results = (await collection.FindAsync(Filter)).First();
                Records.Add(results);
            }

            return Records;
        }

        /// <summary>
        /// Load all records that the value of the specified field matches with the given value.
        /// </summary>
        /// <param name="table">The collection to load all the documents from.</param>
        /// <param name="Field">The name of the field.</param>
        /// <param name="value">The value that the specified field should have.</param>
        /// <typeparam name="T">The schema model/schema structure (E.g a POCO class) to load the records as.</typeparam>
        /// <returns>All the records from the given collection (table) that the value of the specified field matches with the given value as a <see cref="List{T}"/>.</returns>
        public List<T> LoadAnyRecByFieldAndValue<T>(string table, string Field, string Value)
        {
            var collection = db.GetCollection<T>(table);
            var Filter = Builders<T>.Filter.Eq(Field, Value);

            return collection.Find(Filter).ToList();
        }

        public List<T> LoadAnyRecByFieldAndValue<T>(IMongoCollection<T> collection, string Field, string Value)
        {
            var Filter = Builders<T>.Filter.Eq(Field, Value);

            return collection.Find(Filter).ToList();
        }

        /// <summary>
        /// Asynchronously load all records that the value of the specified field matches with the given value.
        /// </summary>
        /// <param name="table">The collection to load all the documents from.</param>
        /// <param name="Field">The name of the field.</param>
        /// <param name="value">The value that the specified field should have.</param>
        /// <typeparam name="T">The schema model/schema structure (E.g a POCO class) to load the records as.</typeparam>
        /// <returns>All the records from the given collection (table) that the value of the specified field matches with the given value as a <see cref="List{T}"/>.</returns>
        public async Task<List<T>> LoadAnyRecByFieldAndValueAsync<T>(string table, string Field, string value)
        {
            var collection = db.GetCollection<T>(table);
            var Filter = Builders<T>.Filter.Eq(Field, value);

            return await collection.FindAsync(Filter).ConfigureAwait(false).GetAwaiter().GetResult().ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<List<T>> LoadAnyRecByFieldAndValueAsync<T>(IMongoCollection<T> collection, string Field,
            string value)
        {
            var Filter = Builders<T>.Filter.Eq(Field, value);

            return await collection.FindAsync(Filter).ConfigureAwait(false).GetAwaiter().GetResult().ToListAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Similar to <see cref="LoadAnyRecByFieldAndValueAsync{T}"/> but instead of loading all record, it loads only the first (one) record from the given collection.
        /// Asynchronously load one (the first) record that the value of the specified field matches with the given value.
        /// </summary>
        /// <param name="table">The collection to load all the documents from.</param>
        /// <param name="Field">The name of the field to get it's values.</param>
        /// <param name="value">The value that the specified field should have.</param>
        /// <typeparam name="T">The schema model/schema structure (E.g a POCO class) to load the records as.</typeparam>
        /// <returns>The first (one) record from the given collection (table) that the value of the specified field matches with the given value.
        /// <br />
        /// <br>Results return as a <see cref="List{T}"/>.</br>
        /// </returns>
        public async Task<T> LoadOneRecByFieldAndValueAsync<T>(string table, string Field, string value)
        {
            var collection = db.GetCollection<T>(table);
            var Filter = Builders<T>.Filter.Eq(Field, value);

            return (await collection.FindAsync(Filter)).FirstAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Similar to <see cref="LoadAnyRecByFieldAndValueAsync{T}"/> but instead of loading all records, it loads only the first (one) record from the given collection.
        /// Asynchronously load one (the first) record that the value of the specified field matches with the given value.
        /// </summary>
        /// <param name="table">The collection to load all the documents from.</param>
        /// <param name="Field">The name of the field to get it's values.</param>
        /// <param name="value">The value that the specified field should have.</param>
        /// <typeparam name="T">The schema model/schema structure (E.g a POCO class) to load the records as.</typeparam>
        /// <returns>The first (one) record from the given collection (table) that the value of the specified field matches with the given value.
        /// <br />
        /// <br>Results return as a <see cref="List{T}"/>.</br>
        /// </returns>
        public async Task<T> LoadOneRecByFieldAndValueAsync<T>(string table, string Field, ulong value)
        {
            var collection = db.GetCollection<T>(table);
            var Filter = Builders<T>.Filter.Eq(Field, value);

            return (await collection.FindAsync(Filter)).FirstAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Similar to <see cref="LoadAnyRecByFieldAndValueAsync{T}"/> but instead of loading all records, it loads only the first (one) record from the given collection.
        /// Asynchronously load one (the first) record that the value of the specified field matches with the given value.
        /// </summary>
        /// <param name="table">The collection to load all the documents from.</param>
        /// <param name="Field">The name of the field to get it's values.</param>
        /// <param name="value">The value that the specified field should have.</param>
        /// <typeparam name="T">The schema model/schema structure (E.g a POCO class) to load the records as.</typeparam>
        /// <returns>The first (one) record from the given collection (table) that the value of the specified field matches with the given value.
        /// <br />
        /// <br>Results return as a <see cref="List{T}"/>.</br>
        /// </returns>
        public async Task<T> LoadOneRecByFieldAndValueAsync<T>(string table, string Field, bool value)
        {
            var collection = db.GetCollection<T>(table);
            var Filter = Builders<T>.Filter.Eq(Field, value);

            return (await collection.FindAsync(Filter)).FirstAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<T> LoadOneRecByFieldAndValueAsync<T>(IMongoCollection<T> collection, string Field, string value)
        {
            var Filter = Builders<T>.Filter.Eq(Field, value);

            return (await collection.FindAsync(Filter)).FirstAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<T> LoadOneRecByFieldAndValueAsync<T>(IMongoCollection<T> collection, string Field, ulong value)
        {
            var Filter = Builders<T>.Filter.Eq(Field, value);

            return (await collection.FindAsync(Filter)).FirstAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<T> LoadOneRecByFieldAndValueAsync<T>(IMongoCollection<T> collection, string Field, bool value)
        {
            var Filter = Builders<T>.Filter.Eq(Field, value);

            return (await collection.FindAsync(Filter)).FirstAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }


        /// <summary>
        /// NOT SURE ABOUT THE FOLLOWING, BUT IT IS STILL A FEATURE: Similar to <see cref="LoadRecsFieldsValuesByField{T}"/> but in comparison, this method is supposedly FASTER, as it tells the MongoDB query FROM THE START to only get one single specific field values from all documents in the given table (collection) asynchronously, instead of loading the entire collection and all the documents and then selecting (filtering) what field to return back.
        /// </summary>
        /// <param name="table">The collection to load all the documents from.</param>
        /// <param name="Field">The name of the field to get it's values.</param>
        /// <typeparam name="T">The schema model/schema structure (E.g a POCO class) to load the records as.</typeparam>
        /// <returns>All the fields values of records from the given collection (table) as a <see cref="List{T}"/>.</returns>
        public async Task<List<string>> GetOnlySpecificFieldValuesAsync<T>(string table, string Field)
        {
            var collection = db.GetCollection<T>(table);
            var FieldValues = await (await collection.DistinctAsync<string>(Field, FilterDefinition<T>.Empty)).ToListAsync();

            return FieldValues;
        }

        public async Task<List<string>> GetOnlySpecificFieldValuesAsync<T>(IMongoCollection<T> collection, string Field)
        {
            var FieldValues = await (await collection.DistinctAsync<string>(Field, FilterDefinition<T>.Empty)).ToListAsync();

            return FieldValues;
        }

        public async Task<(bool Exists, T FoundRecord)> DoesExistAsync<T>(string table, string Field, string value)
        {
            var collection = db.GetCollection<T>(table);
            var Filter = Builders<T>.Filter.Eq(Field, value);

            var Docs = await (await collection.FindAsync(Filter)).ToListAsync();

            if (Docs.Count == 1)
            {
                return (true, Docs.FirstOrDefault());
            }
            else
            {
                return (false, default);
            }
        }

        public async Task<(bool Exists, T FoundRecord)> DoesExistAsync<T>(IMongoCollection<T> collection, string Field, string value)
        {
            var Filter = Builders<T>.Filter.Eq(Field, value);

            var Docs = await (await collection.FindAsync(Filter)).ToListAsync();

            if (Docs.Count == 1)
            {
                return (true, Docs.FirstOrDefault());
            }
            else
            {
                return (false, default);
            }
        }

        public async Task<(bool Exists, List<T> FoundRecords)> DoManyExistAsync<T>(string table, string Field, string value)
        {
            var collection = db.GetCollection<T>(table);
            var Filter = Builders<T>.Filter.Eq(Field, value);

            var Docs = await (await collection.FindAsync(Filter)).ToListAsync();

            if (Docs.Count > 0)
            {
                return (true, Docs);
            }
            else
            {
                return (false, default);
            }
        }



        public async Task<bool> DoesExistInArrayAsync<T>(string table, string ArrayField, string value)
        {
            var collection = db.GetCollection<T>(table);
            var FieldValues = await (await collection.DistinctAsync<string>(ArrayField, FilterDefinition<T>.Empty)).ToListAsync();

            bool Exists = false;
            foreach (var item in FieldValues)
            {
                if (item == value)
                {
                    Exists = true;
                }
                else
                {
                    Exists = false;
                }
            }
            return Exists;
        }

        #endregion

        #region Delete Records
        /// <summary>
        /// Asynchronously delete a single record (document) that the value of it's given field matches the given value [E.g the record id (The _id field of a document)]
        /// </summary>
        /// <param name="table">The collection to delete the record from.</param>
        /// <param name="Field">The field to look for when searching for the record [E.g the record id (The _id field of a document)].</param>
        /// <param name="value">The value of the field to look for when searching for the record.</param>
        /// <typeparam name="T">The schema model/schema structure (E.g a POCO class) of the record to delete.</typeparam>
        /// <returns>The documment that was deleted.</returns>
        public async Task<T> DeleteOneRecByFieldAndValueAsync<T>(string table, string Field, string value)
        {
            var collection = db.GetCollection<T>(table);
            var Filter = Builders<T>.Filter.Eq(Field, value);

            return await collection.FindOneAndDeleteAsync(Filter).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously delete a single record (document) that the value of it's given field matches the given value [E.g the record id (The _id field of a document)]
        /// </summary>
        /// <param name="table">The collection to delete the record from.</param>
        /// <param name="Field">The field to look for when searching for the record [E.g the record id (The _id field of a document)].</param>
        /// <param name="value">The value of the field to look for when searching for the record.</param>
        /// <typeparam name="T">The schema model/schema structure (E.g a POCO class) of the record to delete.</typeparam>
        /// <returns>The documment that was deleted.</returns>
        public async Task<T> DeleteOneRecByFieldAndValueAsync<T>(string table, string Field, object value)
        {
            var collection = db.GetCollection<T>(table);
            var Filter = Builders<T>.Filter.Eq(Field, value);

            return await collection.FindOneAndDeleteAsync(Filter).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously delete a single record (document) that the value of it's given field matches the given value [E.g the record id (The _id field of a document)]
        /// </summary>
        /// <param name="collection">The collection to delete the record from.</param>
        /// <param name="Field">The field to look for when searching for the record [E.g the record id (The _id field of a document)].</param>
        /// <param name="value">The value of the field to look for when searching for the record.</param>
        /// <typeparam name="T">The schema model/schema structure (E.g a POCO class) of the record to delete.</typeparam>
        /// <returns>The documment that was deleted.</returns>
        public async Task<T> DeleteOneRecByFieldAndValueAsync<T>(IMongoCollection<T> collection, string Field, string value)
        {
            var Filter = Builders<T>.Filter.Eq(Field, value);
            return await collection.FindOneAndDeleteAsync(Filter).ConfigureAwait(false);
        }

        public async Task<List<T>> DeleteManyAsync<T>(string table, string Field, List<string> Values)
        {
            var DeletedDocs = new List<T>();

            foreach (var Value in Values)
            {
                var Doc = await DeleteOneRecByFieldAndValueAsync<T>(table, Field, Value);
                DeletedDocs.Add(Doc);
            }

            return DeletedDocs;
        }

        public async Task<List<T>> DeleteManyAsync<T>(string table, string Field, List<Guid> Values)
        {
            var DeletedDocs = new List<T>();

            foreach (var Value in Values)
            {
                var Doc = await DeleteOneRecByFieldAndValueAsync<T>(table, Field, Value);
                DeletedDocs.Add(Doc);
            }

            return DeletedDocs;
        }

        public async Task<List<T>> DeleteManyAsync<T>(IMongoCollection<T> collection, string Field, List<string> Values)
        {
            var DeletedDocs = new List<T>();

            foreach (var Value in Values)
            {
                var Doc = await DeleteOneRecByFieldAndValueAsync<T>(collection, Field, Value);
                DeletedDocs.Add(Doc);
            }

            return DeletedDocs;
        }

        #endregion

        #region Converters
        public T BsonDocumentToJson<T>(string table, BsonDocument bsonDocument)
        {
            var collection = db.GetCollection<T>(table);
            T document;
            using (var jsonReader = new global::MongoDB.Bson.IO.JsonReader(bsonDocument.ToJson()))
            {
                var context = BsonDeserializationContext.CreateRoot(jsonReader);
                document = collection.DocumentSerializer.Deserialize(context);
            }

            return document;
        }

        public T BsonDocumentToType<T>(IMongoCollection<T> collection, BsonDocument bsonDocument)
        {
            T document;
            using (var jsonReader = new global::MongoDB.Bson.IO.JsonReader(bsonDocument.ToJson()))
            {
                var context = BsonDeserializationContext.CreateRoot(jsonReader);
                document = collection.DocumentSerializer.Deserialize(context);
            }

            return document;
        }

        public static string BsonDocumentToJsonString<T>(BsonDocument bsonDocument)
        {
            T e = default;
            MemoryStream ms = new MemoryStream(bsonDocument.ToBson());
            using (Newtonsoft.Json.Bson.BsonReader reader = new Newtonsoft.Json.Bson.BsonReader(ms))
            {
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                e = serializer.Deserialize<T>(reader);

            }
            return e.ToJson();
        }

        #endregion


        #region MongoSync - MongoDB To MongoDB Database syncing

        /// <summary>
        /// The backup settings for <see cref="MongoCRUD.BackUpDbServerToLocalFileAsync{T}"/>.
        /// </summary>
        public class MongoBackUpOptions
        {
            /// <summary>
            /// The <see cref="IMongoCollection{TDocument}"/> to backup the records from.
            /// </summary>
            public string FromTable { get; set; }

            public string ToTable { get; set; }

            /// <summary>
            /// The backup interval in minutes for backing up the <see cref="IMongoCollection{TDocument}"/>.
            /// [Defaults to 5 minutes]
            /// </summary>
            public int BackupIntervalInMinutes { get; set; } = 5;
        }

        public static string ConfigFile = $"{Directory.GetCurrentDirectory()}\\config.json";

        public static string FromCollectionName { get; set; }
        public static string ToCollectionName { get; set; }
        public static int BackupIntervalInMinutes { get; set; }

        public static async void OnConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                WasChanged = true;
                Output.WriteLine(ConsoleColor.Blue, $"Config file changed! Applying new changes!");
                var NewConfig = await ReadConfigAsync(ConfigFile);

                FromCollectionName = NewConfig.FromCollectionName;
                ToCollectionName = NewConfig.ToCollectionName;
                BackupIntervalInMinutes = NewConfig.BackupIntervalInMinutes;
            }
            catch (Exception EX)
            {
                EX.OutputBigExceptionError();
                Output.WriteLine(ConsoleColor.Magenta,
                    "Since you added the config file watcher maybe the error was caused by a recent change for one of the parameters of the config file.\n" +
                    "Please make sure all of the parameters are valid and that you didn't edit or remove a connection string/database name from the config file.\n" +
                    "As warned before, you may can only change the COLLECTION names and/or the update interval!\n" +
                    "\n" +
                    "If the problem was from the config file and you think it is now fixed, press any key to retry and continue the syncing process");

                Console.ReadKey();
            }
        }

        public static async Task<ConfigJson> ReadConfigAsync(string ConfigPath)
        {
            //ConfigJson? configJson = default;
            // try
            // {
            var json = string.Empty;
            using (var fs = File.OpenRead(ConfigPath))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            var configJson = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfigJson>(json);

            return configJson;
        }

        public async Task BackUpMongoDBToMongoDB<T>(MongoCRUD ToDB, MongoBackUpOptions BackUpOptions,
            FileSystemWatcher ConfigWatcher = null)
        {
            Output.WriteLine(ConsoleColor.DarkYellow,
                $"\nInitialized database backup procedure for {BackUpOptions.FromTable}.\n");

            if (ConfigWatcher != null)
            {
                ConfigWatcher.NotifyFilter = NotifyFilters.LastAccess
                                             | NotifyFilters.LastWrite
                                             | NotifyFilters.FileName
                                             | NotifyFilters.DirectoryName;

                ConfigWatcher.Filter = "config.json";

                ConfigWatcher.Changed += OnConfigFileChanged;
                ConfigWatcher.EnableRaisingEvents = true;

                Output.WriteLine(ConsoleColor.Cyan,
                    $"Watching config file {ConfigWatcher.Path}. If a parameter changes in the config file the changes will apply in real time\n" +
                    $"NOTE: This DOES NOT apply on changing the connection strings nor the databases names.\n" +
                    $"Use this to change the collection names and/or the update interval when needed.");
            }

            FromCollectionName = BackUpOptions.FromTable;
            ToCollectionName = BackUpOptions.ToTable;
            BackupIntervalInMinutes = BackUpOptions.BackupIntervalInMinutes;

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(BackupIntervalInMinutes).TotalMilliseconds;

            await Task.Delay(6 * 1000);


            int Tries = 0;
        Start:
            // var timer = new System.Threading.Timer(async (e) =>
            // {
            try
            {
                try
                {
                    var Collection = await GetCollectionAsync<T>(FromCollectionName);

                    long Updates = 0;
                    List<string> ExistingDocsIDs = new List<string>();
                    await Collection.Find(new BsonDocument())
                        .ForEachAsync(async (document) =>
                        {
                            var DocID = document.ToBsonDocument().GetElement("_id").Value.ToString();
                            ReplaceOneResult Result;

                            ExistingDocsIDs.Add(DocID);
                            Result = await ToDB.UpsertAsync(ToCollectionName, DocID, document);
                            Updates += Result.ModifiedCount;
                        });

                    var ToDBDocsIDs = await ToDB.GetOnlySpecificFieldValuesAsync<T>(ToCollectionName, "_id");
                    var Difference = await MethodExtensions.DifferenceBetweenLists(ToDBDocsIDs, ExistingDocsIDs);
                    List<T> HowManyWereDeleted = new List<T>();
                    if (Difference.IsDifferent)
                    {
                        HowManyWereDeleted =
                            await ToDB.DeleteManyAsync<T>(ToCollectionName, "_id", Difference.DifferentItems);
                    }

                    Output.WriteLine(ConsoleColor.Green,
                        $"\n[{DateTime.Now}] Successfully synced the '{FromCollectionName}' collection to the '{ToCollectionName}' collection! {Updates} docs were updated (modified), {HowManyWereDeleted} docs were deleted, {Updates + HowManyWereDeleted.Count} total changes were applied and synced during this process. Next update is scheduled to {DateTime.Now.AddMinutes(BackupIntervalInMinutes)} [{BackupIntervalInMinutes} minutes from now].");
                }
                catch (Exception e)
                {
                    e.OutputBigExceptionError();
                    // if (Tries <= 5)
                    // {
                    //     Console.WriteLine("EEEEEEEEEEEEEEEEE 676");
                    //     Tries++;
                    //     Output.WriteLine(ConsoleColor.Red,
                    //         $"An errored while trying to connect to the {FromCollectionName} Mongo database. [This is attempt number {Tries}/5, Waiting 5 seconds before reconnecting].");
                    //     Task.Delay(5 * 1000);
                    //     goto Start;
                    // }
                    // else
                    // {
                    //     DebuggingUtils.OutputBigError(
                    //         "Too many failed attempts... Press any key to close the program and maybe try again. If the problem consists, consider checking you connection string or 'allowed to access' IPs to see if you are not being blocked.");
                    //     Console.ReadKey();
                    //     Environment.Exit(69);
                    // }
                }

            }
            catch (Exception EXC)
            {
                EXC.OutputBigExceptionError();
                if (ConfigWatcher != null && WasChanged == true)
                {
                    Output.WriteLine(ConsoleColor.Magenta,
                        "Since you added the config file watcher maybe the error was caused by a recent change for one of the parameters of the config file.\n" +
                        "Please make sure all of the parameters are valid and that you didn't edit or remove a connection string/database name from the config file.\n" +
                        "As warned before, you may can only change the COLLECTION names and/or the update interval!\n" +
                        "\n" +
                        "If the problem was from the config file and you think it is now fixed, press any key to retry and continue the syncing process");

                    Console.ReadKey();
                    WasChanged = false;
                    goto Start;
                }
            }
            // }, null, startTimeSpan, periodTimeSpan);

            await Task.Delay((int)TimeSpan.FromMinutes(BackupIntervalInMinutes).TotalMilliseconds);
            goto Start;
        }
        #endregion

        #region LiteDB Integration

        /// <summary>
        /// Connection type for the <see cref="LiteDBConnectionString"/>.
        /// </summary>
        public enum ConnectionType
        {
            /// <summary>
            /// Engine will open the datafile in exclusive mode and will keep it open until Dispose(). The datafile cannot be opened by another process. This is the recommended mode because it’s faster and cacheable.
            /// </summary>
            Direct,

            /// <summary>
            /// Engine will be close the datafile after each operation. Locks are made using Mutex. This is more expensive but you can open same file from multiple processes.
            /// </summary>
            Shared
        }

        #region Connection string builder
        /// <summary>
        /// LiteDatabase can be initialized using a string connection, with key1=value1; key2=value2; ... syntax.
        /// Values can be quoted (" or ') if they contain special characters (like ; or =).
        /// More info about this can be found <a href="http://www.litedb.org/docs/connection-string/">here on the official website of <see cref="LiteDB"/>.</a>
        /// </summary>
        public class LiteDBConnectionString
        {
            /// <summary>
            /// Full or relative path to the <see cref="LiteDatabase"/> datafile. Supports :memory: for memory database or :temp: for in disk temporary database (file will deleted when database is closed)
            /// [Defaults to: .\Bin\Debug\LocalLiteDb\LocalMongoBackupLiteDB.db]
            /// </summary>
            public string Filename { get; set; } =
                $"{Directory.GetCurrentDirectory()}\\LocalLiteDb\\LocalMongoBackupLiteDB.db";

            /// <summary>
            /// Connection type (“direct” or “shared”). Defaults to 'direct'
            /// </summary>
            public ConnectionType ConnectionType { get; set; } = ConnectionType.Direct;

            /// <summary>
            /// Encrypt (using AES) your datafile with a password. [Defaults to null (no encryption)]
            /// </summary>
            public string Password { get; set; } = string.Empty;

            /// <summary>
            /// Initial size for the datafile (<see cref="string"/> supports “KB”, “MB” and “GB”). [Defaults to 0]
            /// </summary>
            public string InitialSize { get; set; } = "0";

            /// <summary>
            /// Open datafile in read-only mode. [Defaults to false]
            /// </summary>
            public bool ReadOnly { get; set; } = false;

            /// <summary>
            /// Check if datafile is of an older version and upgrade it before opening. [Defaults to false]
            /// </summary>
            public bool Upgrade { get; set; } = false;


            public string ConnectionStringBuilder()
            {
                StringBuilder ConnectionString = new StringBuilder();

                // <---! Filename !---> \\
                ConnectionString.Append($"Filename=\"{Filename}\"");

                // <---! Connection type !---> \\
                switch (ConnectionType)
                {
                    case ConnectionType.Direct:
                        ConnectionString.Append(";Connection=\"direct\"");
                        break;
                    case ConnectionType.Shared:
                        ConnectionString.Append(";Connection=\"shared\"");
                        break;
                }

                // <---! Password !---> \\
                if (Password != String.Empty) ConnectionString.Append($";Password=\"{Password}\"");

                // <---! InitialSize !---> \\
                if (InitialSize != "0") ConnectionString.Append($";InitialSize={InitialSize}");

                // <---! ReadOnly !---> \\
                if (ReadOnly == true) ConnectionString.Append($";ReadOnly=true");

                // <---! Upgrade !---> \\
                if (Upgrade == true) ConnectionString.Append($";Upgrade=true");


                return ConnectionString.ToString();
            }
        }
        #endregion




        /// <summary>
        /// Asynchronously backup all documents data from a MongoDB database collection to a <see cref="LiteDatabase"/> database using <see cref="LiteDB"/>.
        /// </summary>
        /// <param name="BackupOptions">The <see cref="MongoBackUpOptions"/> for this backup method.</param>
        /// <param name="liteDatabase">The <see cref="LiteDBConnectionString"/> connection string parameters options for this database.</param>
        /// <typeparam name="T">Initialize to the collection to read from.</typeparam>
        /// <returns>All the data of the specified collection to a Local <see cref="LiteDatabase"/> database.</returns>
        public async Task BackUpMongoDbServerToLiteDB<T>(MongoBackUpOptions BackupOptions, LiteDB.LiteDatabase liteDatabase)
        {
            var BackupInterval = BackupOptions.BackupIntervalInMinutes;
            var FromTable = BackupOptions.FromTable;
            var ToTable = BackupOptions.ToTable;

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(BackupInterval);

            var collection = db.GetCollection<T>(FromTable); // The MongoDB collection

            Output.WriteLine(ConsoleColor.DarkYellow, $"\nInitialized database backup procedure for {FromTable}.\n");
            if (FromTable.Contains("-")) FromTable.Replace("-", "_");

            var timer = new System.Threading.Timer(async (e) =>
            {
                var LocalCol = liteDatabase.GetCollection<T>(FromTable); // The LiteDB collection
                List<T> Docs = new List<T>();

                await collection.Find(new BsonDocument())
                    .ForEachAsync(async (document) => { Docs.Add(document); });

                try
                {
                    var DocsBefore = LocalCol.Count();
                    LocalCol.DeleteAll();
                    var DocsNow = LocalCol.InsertBulk(Docs);
                    var HowManyUpdates = DocsNow - DocsBefore;

                    if (HowManyUpdates != 0)
                        Output.WriteLine(ConsoleColor.Green,
                            $"\nSuccessfully updated the '{LocalCol.Name}' collection! {HowManyUpdates} documents were updated during this process.");
                    else
                        Output.WriteLine(ConsoleColor.Yellow,
                            "Good news! The databases are already synced! No data had to be updated.");
                }
                catch (LiteDB.LiteException LE)
                {
                    Output.WriteLine(ConsoleColor.Red, LE.Message + " - InnerException: " + LE.InnerException);
                }
            }, null, startTimeSpan, periodTimeSpan);
        }
        #endregion



    }
}