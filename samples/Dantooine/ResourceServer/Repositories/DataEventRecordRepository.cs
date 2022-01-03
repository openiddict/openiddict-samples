using System;
using System.Collections.Generic;
using System.Linq;
using ResourceServer.Model;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ResourceServer.Repositories
{
    public class DataEventRecordRepository
    {
        private readonly DataEventRecordContext _context;
        private readonly ILogger _logger;

        public DataEventRecordRepository(DataEventRecordContext context, 
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger("DataEventRecordResporitory");
        }

        public List<DataEventRecord> GetAll()
        {
            _logger.LogCritical("Getting a the existing records");
            var data =  _context.DataEventRecords.ToList();

            return data;
        }

        public DataEventRecord Get(long id)
        {
            var dataEventRecord = _context.DataEventRecords.First(t => t.Id == id);
            return dataEventRecord;
        }

        [HttpPost]
        public void Post(DataEventRecord dataEventRecord )
        {
            if(string.IsNullOrWhiteSpace(dataEventRecord.Timestamp))
            {
                dataEventRecord.Timestamp = DateTime.UtcNow.ToString("o");
            };
            _context.DataEventRecords.Add(dataEventRecord);
            _context.SaveChanges();
        }

        public void Put(long id, [FromBody]DataEventRecord dataEventRecord)
        {
            dataEventRecord.Timestamp = DateTime.UtcNow.ToString("o");
            _context.DataEventRecords.Update(dataEventRecord);
            _context.SaveChanges();
        }

        public void Delete(long id)
        {
            var entity = _context.DataEventRecords.First(t => t.Id == id);
            _context.DataEventRecords.Remove(entity);
            _context.SaveChanges();
        }
    }
}
