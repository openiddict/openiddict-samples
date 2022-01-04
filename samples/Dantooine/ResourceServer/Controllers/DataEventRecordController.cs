using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceServer.Model;
using ResourceServer.Repositories;

namespace ResourceServer.Controllers
{
    [Authorize("dataEventRecordsPolicy")]
    [Route("api/[controller]")]
    public class DataEventRecordsController : Controller
    {
        private readonly DataEventRecordRepository _dataEventRecordRepository;

        public DataEventRecordsController(DataEventRecordRepository dataEventRecordRepository)
        {
            _dataEventRecordRepository = dataEventRecordRepository;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_dataEventRecordRepository.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult Get(long id)
        {
            return Ok(_dataEventRecordRepository.Get(id));
        }

        [HttpPost]
        public void Post([FromBody] DataEventRecord value)
        {
            _dataEventRecordRepository.Post(value);
        }

        [HttpPut("{id}")]
        public void Put(long id, [FromBody] DataEventRecord value)
        {
            _dataEventRecordRepository.Put(id, value);
        }

        [HttpDelete("{id}")]
        public void Delete(long id)
        {
            _dataEventRecordRepository.Delete(id);
        }
    }
}
