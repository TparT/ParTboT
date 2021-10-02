using DSharpPlus.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ParTboT.API.Controllers
{

    [Route("api/v1/[controller]")]
    [ApiController]
    public class ChannelController : ControllerBase
    {
        private readonly Bot bot;

        public ChannelController(Bot bot)
        {
            this.bot = bot;
        }

        //// GET: api/<ChannelController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET api/<ChannelController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(ulong id)
        {
            return new JsonResult(JObject.FromObject(await bot.Client.GetChannelAsync(id).ConfigureAwait(false)).ToString());
        }

        // POST api/<ChannelController>
        [HttpPost("{id}")]
        public async Task<IActionResult> Post(ulong id, [FromBody] string value)
        {
            DiscordChannel channel = await bot.Client.GetChannelAsync(id).ConfigureAwait(false);
            return Created(nameof(Get), await channel.SendMessageAsync(value).ConfigureAwait(false));
        }

        //// PUT api/<ChannelController>/5
        //[HttpPut("{id}")]
        //public async Task<IActionResult> Put(int id, [FromBody] string value)
        //{
        //}

        // DELETE api/<ChannelController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Content("YEP DELETED AAAAAA");
        }
    }
}
