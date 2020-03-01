using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GeoDoorServer3.API.Model;
using GeoDoorServer3.CustomService;
using GeoDoorServer3.CustomService.Models;
using GeoDoorServer3.Data;
using GeoDoorServer3.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GeoDoorServer3.API
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly UserDbContext _context;
        private readonly IOpenHabMessageService _openHab;
        private readonly IDataSingleton _iDataSingleton;

        #region Public Methods

        public ValuesController(UserDbContext context, IOpenHabMessageService openHab, IDataSingleton iDataSingleton)
        {
            _context = context;
            _openHab = openHab;
            _iDataSingleton = iDataSingleton;
        }
        
        /// <summary>
        /// POST api/-controller-
        /// POST API to send commands to. See <see cref="Command"/> and also <see cref="CommandItem"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<CommandItem>> PostCommandItem([FromBody]CommandItem item)
        {
            try
            {
                _iDataSingleton.AddErrorLog(new ErrorLog()
                {
                    LogLevel = LogLevel.Debug,
                    MsgDateTime = DateTime.Now,
                    Message = $"{typeof(ValuesController)}:PostCommandItem => {item}"
                });

                if (!CheckCommandItem(item))
                    return NotFound();

                if (item.Command.Equals(Command.CheckUser))
                {
                    if (await CreateUser(item))
                    {
                        return Ok(new CommandItem()
                        {
                            Id = item.Id,
                            Command = item.Command,
                            CommandValue = "User created!"
                        });
                    }
                    return BadRequest(new CommandItem()
                    {
                        Id = item.Id,
                        Command = item.Command,
                        CommandValue = "Couldn't create user!"
                    });
                }
                else
                {
                    if (!await CheckUser(item))
                    {
                        return BadRequest(new CommandItem()
                        {
                            Id = item.Id,
                            Command = item.Command,
                            CommandValue = "User not allowed!"
                        });   
                    }
                }

                return await CommandItemHandler(item);
            }
            catch (Exception e)
            {
                _iDataSingleton.AddErrorLog(new ErrorLog()
                {
                    LogLevel = LogLevel.Error,
                    MsgDateTime = DateTime.Now,
                    Message = $"{typeof(ValuesController)}:PostCommandItem Exception => {e}"
                });
                return NotFound();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This method handles the different commands.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private async Task<ActionResult<CommandItem>> CommandItemHandler(CommandItem item)
        {
            switch (item.Command)
            {
                case Command.OpenDoor:
                    break;
                case Command.OpenGate:
                    switch (_iDataSingleton.GetSystemStatus().GateStatus)
                    {
                        case GateStatus.GateOpen:
                            if (item.CommandValue.Equals(CommandValue.Open.ToString()) ||
                                item.CommandValue.Equals(CommandValue.ForceOpen.ToString()))
                            {
                                return Accepted(new CommandItem()
                                {
                                    Id = item.Id,
                                    Command = item.Command,
                                    CommandValue = CommandValueAnswer.AlreadyOpen.ToString()
                                });
                            }
                            else if (item.CommandValue.Equals(CommandValue.Close.ToString()) ||
                                     item.CommandValue.Equals(CommandValue.ForceClose.ToString()))
                            {
                                _iDataSingleton.GetSystemStatus().IsGateMoving = true;
                                await _openHab.PostData(_iDataSingleton.GatePathValueChange(), "ON", true);
                                return Accepted(new CommandItem
                                {
                                    Id = item.Id,
                                    Command = item.Command,
                                    CommandValue = CommandValueAnswer.GateClosing.ToString()
                                });
                            }
                            break;
                        case GateStatus.GateOpening:
                            if (item.CommandValue.Equals(CommandValue.Open.ToString()) || 
                                item.CommandValue.Equals(CommandValue.ForceOpen.ToString()))
                            {
                                return Accepted(new CommandItem()
                                {
                                    Id = item.Id,
                                    Command = item.Command,
                                    CommandValue = CommandValueAnswer.GateOpening.ToString()
                                });
                            }
                            else if (item.CommandValue.Equals(CommandValue.Close.ToString()))
                            {
                                return Accepted(new CommandItem
                                {
                                    Id = item.Id,
                                    Command = item.Command,
                                    CommandValue = CommandValueAnswer.GateOpening.ToString()
                                });
                            }
                            else if (item.CommandValue.Equals(CommandValue.ForceClose.ToString()))
                            {
                                _iDataSingleton.GetSystemStatus().IsGateMoving = true;
                                await _openHab.PostData(_iDataSingleton.GatePathValueChange(), "ON", true);
                                Thread.Sleep(500);
                                await _openHab.PostData(_iDataSingleton.GatePathValueChange(), "ON", true);
                                return Accepted(new CommandItem
                                {
                                    Id = item.Id,
                                    Command = item.Command,
                                    CommandValue = CommandValueAnswer.GateClosing.ToString()
                                });
                            }
                            break;
                        case GateStatus.GateClosed:
                            if (item.CommandValue.Equals(CommandValue.Open.ToString()) ||
                                item.CommandValue.Equals(CommandValue.ForceOpen.ToString()))
                            {
                                _iDataSingleton.GetSystemStatus().IsGateMoving = true;
                                await _openHab.PostData(_iDataSingleton.GatePathValueChange(), "ON", true);
                                return Accepted(new CommandItem()
                                {
                                    Id = item.Id,
                                    Command = item.Command,
                                    CommandValue = CommandValueAnswer.GateOpening.ToString()
                                });
                            }
                            else if (item.CommandValue.Equals(CommandValue.Close.ToString()) ||
                                     item.CommandValue.Equals(CommandValue.ForceClose.ToString()))
                            {
                                return Accepted(new CommandItem
                                {
                                    Id = item.Id,
                                    Command = item.Command,
                                    CommandValue = CommandValueAnswer.AlreadyClosed.ToString()
                                });
                            }
                            break;
                        case GateStatus.GateClosing:
                            if (item.CommandValue.Equals(CommandValue.Close.ToString()) || 
                                item.CommandValue.Equals(CommandValue.ForceClose.ToString()))
                            {
                                return Accepted(new CommandItem()
                                {
                                    Id = item.Id,
                                    Command = item.Command,
                                    CommandValue = CommandValueAnswer.GateClosing.ToString()
                                });
                            }
                            else if (item.CommandValue.Equals(CommandValue.Open.ToString()))
                            {
                                return Accepted(new CommandItem
                                {
                                    Id = item.Id,
                                    Command = item.Command,
                                    CommandValue = CommandValueAnswer.GateClosing.ToString()
                                });
                            }
                            else if (item.CommandValue.Equals(CommandValue.ForceOpen.ToString()))
                            {
                                _iDataSingleton.GetSystemStatus().IsGateMoving = true;
                                await _openHab.PostData(_iDataSingleton.GatePathValueChange(), "ON", true);
                                Thread.Sleep(500);
                                await _openHab.PostData(_iDataSingleton.GatePathValueChange(), "ON", true);
                                return Accepted(new CommandItem
                                {
                                    Id = item.Id,
                                    Command = item.Command,
                                    CommandValue = CommandValueAnswer.GateOpening.ToString()
                                });
                            }
                            break;
                        default:
                            return NotFound();
                    }
                    break;
            }
            return NotFound();
        }

        /// <summary>
        /// Check only allowed values for each Command in the CommandItem.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Returns true if values are correct otherwise false.</returns>
        private bool CheckCommandItem(CommandItem item)
        {
            switch (Enum.Parse<Command>(item.Command.ToString()))
            {
                case Command.CheckUser:
                case Command.OpenDoor when item.CommandValue.Equals(CommandValue.Open.ToString()) ||
                                           item.CommandValue.Equals(CommandValue.Close.ToString()):
                case Command.OpenGate when item.CommandValue.Equals(CommandValue.Open.ToString()) ||
                                           item.CommandValue.Equals(CommandValue.Close.ToString()) ||
                                           item.CommandValue.Equals(CommandValue.ForceOpen.ToString()) ||
                                           item.CommandValue.Equals(CommandValue.ForceClose.ToString()):
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the user exists and has access rights.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private async Task<bool> CheckUser(CommandItem item)
        {
            if (_context.Users.Any(u => u.PhoneId.Equals(item.Id)))
            {
                User user = await _context.Users.SingleAsync(u => u.PhoneId.Equals(item.Id));
                user.LastConnection = DateTime.Now;
                await _context.SaveChangesAsync();

                if (user.AccessRights == AccessRights.Allowed)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        
        /// <summary>
        /// Creates the user if it doesn't already exists.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private async Task<bool> CreateUser(CommandItem item)
        {
            try
            {
                if (_context.Users.Any(u => u.PhoneId.Equals(item.Id)))
                {
                    return false;
                }

                User user = new User()
                {
                    PhoneId = item.Id,
                    Name = item.CommandValue,
                    AccessRights = AccessRights.NotAllowed,
                    LastConnection = DateTime.Now
                };
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                _iDataSingleton.AddErrorLog(new ErrorLog()
                {
                    LogLevel = LogLevel.Error,
                    MsgDateTime = DateTime.Now,
                    Message = $"{typeof(ValuesController)}:CreateUser Exception => {e}"
                });
                return false;
            }
        }

        #endregion

        #region API Examples

        // PUT api/<controller>/5
        //[HttpPut("{id}")]
        //public void Put(int id, string name, [FromBody] string value)
        //{
        //}

        // DELETE api/<controller>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id, string name)
        //{
        //}
        
        // GET: api/<controller>
        //[HttpGet]
        // public async Task<ActionResult<CommandItem>> Get()
        // {
        // }

        // GET api/<controller>/command/5
        //[HttpGet("command/{id}")]
        //public async Task<ActionResult<CommandItem>> GetCommandItem(int id)
        //{
        //}

        #endregion
    }
}
