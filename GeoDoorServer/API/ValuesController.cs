using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeoDoorServer.API.Model;
using GeoDoorServer.Areas.Identity.Model;
using GeoDoorServer.CustomService;
using GeoDoorServer.CustomService.Models;
using GeoDoorServer.Data;
using GeoDoorServer.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace GeoDoorServer.API
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserDbContext _context;
        private readonly IOpenHabMessageService _openHab;
        private readonly IDataSingleton _iDataSingleton;

        private Timer _autoCloseTimer;
        private int _autoCloseTimout;

        #region Public Methods

        public ValuesController(UserDbContext context, IOpenHabMessageService openHab, IDataSingleton iDataSingleton,
            SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _openHab = openHab;
            _iDataSingleton = iDataSingleton;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;

            _autoCloseTimout = 5000;

            _autoCloseTimer = new Timer();
            _autoCloseTimer.Interval = _autoCloseTimout;
            _autoCloseTimer.Elapsed += async (sender, args) =>
            {
                _autoCloseTimer.Stop();
                _iDataSingleton.GetSystemStatus().IsGateMoving = true;
                await _openHab.PostData(_iDataSingleton.GetSettings().GateOpenHabLink, "ON", true);
                _iDataSingleton.GetSystemStatus().IsAutoMode = false;
            };
        }

        [HttpPost("register")]
        public async Task<ActionResult<AnswerModel>> PostRegister([FromBody] AuthModel auth)
        {
            try
            {
                if (!_context.Users.Any(u => u.Name.Equals(auth.Name)))
                {
                    return NotFound(new AnswerModel()
                    {
                        Answer = "User not found!"
                    });
                }

                User user = await _context.Users.SingleAsync(u => u.Name.Equals(auth.Name));
                user.LastConnection = DateTime.Now;
                await _context.SaveChangesAsync();

                if (user.AccessRights == AccessRights.Register)
                {
                    string md5HashText = GetMd5Hash(user.Name + ":" + DateTime.UtcNow.ToString());
                    user.PhoneId = md5HashText;
                    user.AccessRights = AccessRights.Allowed;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return NotFound(new AnswerModel()
                    {
                        Answer = "User not allowed to register!"
                    });
                }

                return Ok(new AnswerModel()
                {
                    Answer = "User successfully registered!",
                    Data = user.PhoneId
                });
            }
            catch (Exception e)
            {
                _iDataSingleton.AddErrorLog(new ErrorLog()
                {
                    LogLevel = LogLevel.Error,
                    MsgDateTime = DateTime.Now,
                    Message = $"{typeof(ValuesController)}:PostCommandItem Exception => {e}"
                });
                return BadRequest();
            }
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<AnswerModel>> PostLogin([FromBody] AuthModel auth)
        {
            try
            {
                if (_context.Users.Any(u => u.Name.Equals(auth.Name)))
                {
                    User user = await _context.Users.SingleAsync(u => u.Name.Equals(auth.Name));
                    
                    if (user?.Name != null && user.PhoneId != null)
                    {
                        if (!user.AccessRights.Equals(AccessRights.Allowed))
                        {
                            return BadRequest(new AnswerModel()
                            {
                                Answer = "User is not allowed!",
                                Data = user.AccessRights.ToString()
                            });
                        }

                        if (user.Name.Equals(auth.Name) && user.PhoneId.Equals(auth.Md5Hash))
                        {
                            user.LastConnection = DateTime.Now;
                            await _context.SaveChangesAsync();
                    
                            return Ok(new AnswerModel()
                            {
                                Answer = "Login successful!"
                            });
                        }
                    }
                }
                
                return NotFound(new AnswerModel()
                {
                    Answer = "User not found!",
                });
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

        [HttpPost("command")]
        public async Task<ActionResult<AnswerModel>> PostCommandItem([FromBody] CommandItem item)
        {
            try
            {
                if (!CheckCommandItem(item))
                    return NotFound();
                
                ActionResult<AnswerModel> result = await CheckUser(item.Authentication);

                if (result != null)
                {
                    return result;
                }
        
                return CommandItemHandler(item).Result;
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

        //[HttpPost("register")]
        //public async Task<ActionResult<CommandItem>> PostRegister([FromBody] ApiUser item)
        //{
        //    try
        //    {
        //        _iDataSingleton.AddErrorLog(new ErrorLog()
        //        {
        //            LogLevel = LogLevel.Debug,
        //            MsgDateTime = DateTime.Now,
        //            Message = $"{typeof(ValuesController)}:PostRegister => {item}"
        //        });

        //        var user = new ApplicationUser
        //            {UserName = item.User, Email = item.EMail, LastConnection = DateTime.Now};
        //        var result = await _userManager.CreateAsync(user, item.Password);
        //        if (result.Succeeded)
        //        {
        //            var addRoleResult = await _userManager.AddToRoleAsync(user, "ApiUser");
        //            if (!addRoleResult.Succeeded)
        //            {
        //                _iDataSingleton.AddErrorLog(new ErrorLog()
        //                {
        //                    LogLevel = LogLevel.Error,
        //                    MsgDateTime = DateTime.Now,
        //                    Message = $"{typeof(ValuesController)}:PostRegister => Couldn't add User to ApiUser."
        //                });
        //                return BadRequest();
        //            }
        //        }
        //        else
        //        {
        //            _iDataSingleton.AddErrorLog(new ErrorLog()
        //            {
        //                LogLevel = LogLevel.Error,
        //                MsgDateTime = DateTime.Now,
        //                Message = $"{typeof(ValuesController)}:PostRegister => Couldn't create User."
        //            });
        //            return BadRequest();  
        //        }
                
        //        return Ok();
        //    }
        //    catch (Exception e)
        //    {
        //        _iDataSingleton.AddErrorLog(new ErrorLog()
        //        {
        //            LogLevel = LogLevel.Error,
        //            MsgDateTime = DateTime.Now,
        //            Message = $"{typeof(ValuesController)}:PostRegister Exception => {e}"
        //        });
        //        return BadRequest();
        //    }
        //}

        // [HttpPost("login")]
        // public async Task<ActionResult<CommandItem>> PostLogin([FromBody] CommandItem item)
        // {
        //     try
        //     {
        //         _iDataSingleton.AddErrorLog(new ErrorLog()
        //         {
        //             LogLevel = LogLevel.Debug,
        //             MsgDateTime = DateTime.Now,
        //             Message = $"{typeof(ValuesController)}:PostCommandItem => {item}"
        //         });
        //
        //
        //         return Ok();
        //     }
        //     catch (Exception e)
        //     {
        //         _iDataSingleton.AddErrorLog(new ErrorLog()
        //         {
        //             LogLevel = LogLevel.Error,
        //             MsgDateTime = DateTime.Now,
        //             Message = $"{typeof(ValuesController)}:PostCommandItem Exception => {e}"
        //         });
        //         return NotFound();
        //     }
        // }

        #endregion

        #region Private Methods

        /// <summary>
        /// This method handles the different commands.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private async Task<ActionResult<AnswerModel>> CommandItemHandler(CommandItem item)
        {
            switch (item.Command)
            {
                case Command.OpenDoor:
                    await _openHab.PostData(_iDataSingleton.GetSettings().DoorOpenHabLink, "ON", true);
                    break;
                case Command.OpenGate:
                    switch (_iDataSingleton.GetSystemStatus().GateStatus)
                    {
                        case GateStatus.GateOpen:
                            if (_iDataSingleton.GetSystemStatus().IsAutoMode)
                            {
                                if (item.CommandValue.Equals(CommandValue.Open))
                                {
                                    return Accepted(new AnswerModel()
                                    {
                                        Answer = CommandValueAnswer.AlreadyOpen + " but gate is closing automatically!"
                                    });
                                }

                                if (item.CommandValue.Equals(CommandValue.ForceOpen))
                                {
                                    _autoCloseTimer.Stop();
                                    _iDataSingleton.GetSystemStatus().IsAutoMode = false;
                                    return Accepted(new AnswerModel()
                                    {
                                        Answer = CommandValueAnswer.AlreadyOpen + " but auto gate closing was turned off!"
                                    });
                                }

                                if (item.CommandValue.Equals(CommandValue.Close))
                                {
                                    return Accepted(new AnswerModel()
                                    {
                                        Answer = CommandValueAnswer.AlreadyOpen + " but gate will close after auto mode timeout!"
                                    });
                                }
                                
                                if (item.CommandValue.Equals(CommandValue.ForceClose))
                                {
                                    _autoCloseTimer.Stop();
                                    _iDataSingleton.GetSystemStatus().IsAutoMode = false;
                                    _iDataSingleton.GetSystemStatus().IsGateMoving = true;
                                    await _openHab.PostData(_iDataSingleton.GetSettings().GateOpenHabLink, "ON", true);
                                    return Accepted(new AnswerModel()
                                    {
                                        Answer = CommandValueAnswer.GateClosing + " auto gate closing was turned off!"
                                    });
                                }
                            }

                            if (item.CommandValue.Equals(CommandValue.Open) ||
                                item.CommandValue.Equals(CommandValue.ForceOpen))
                            {
                                return Accepted(new AnswerModel()
                                {
                                    Answer = CommandValueAnswer.AlreadyOpen.ToString()
                                });
                            }
                            else if (item.CommandValue.Equals(CommandValue.Close) ||
                                     item.CommandValue.Equals(CommandValue.ForceClose))
                            {
                                _iDataSingleton.GetSystemStatus().IsGateMoving = true;
                                await _openHab.PostData(_iDataSingleton.GetSettings().GateOpenHabLink, "ON", true);
                                return Accepted(new AnswerModel()
                                {
                                    Answer = CommandValueAnswer.GateClosing.ToString()
                                });
                            }

                            break;
                        case GateStatus.GateOpening:
                            if (_iDataSingleton.GetSystemStatus().IsAutoMode)
                            {
                                if (item.CommandValue.Equals(CommandValue.Open))
                                {
                                    return Accepted(new AnswerModel()
                                    {
                                        Answer = CommandValueAnswer.AlreadyOpening + " but gate is closing automatically!"
                                    });
                                }

                                if (item.CommandValue.Equals(CommandValue.ForceOpen))
                                {
                                    _autoCloseTimer.Stop();
                                    _iDataSingleton.GetSystemStatus().IsAutoMode = false;
                                    return Accepted(new AnswerModel()
                                    {
                                        Answer = CommandValueAnswer.AlreadyOpening + " but auto gate closing was turned off!"
                                    });
                                }

                                if (item.CommandValue.Equals(CommandValue.Close))
                                {
                                    return Accepted(new AnswerModel()
                                    {
                                        Answer = CommandValueAnswer.AlreadyOpening + " but gate will close after auto mode timeout!"
                                    });
                                }
                                
                                if (item.CommandValue.Equals(CommandValue.ForceClose))
                                {
                                    _autoCloseTimer.Stop();
                                    _iDataSingleton.GetSystemStatus().IsAutoMode = false;
                                    _iDataSingleton.GetSystemStatus().IsGateMoving = true;
                                    await _openHab.PostData(_iDataSingleton.GetSettings().GateOpenHabLink, "ON", true);
                                    Thread.Sleep(500);
                                    await _openHab.PostData(_iDataSingleton.GetSettings().GateOpenHabLink, "ON", true);
                                    return Accepted(new AnswerModel()
                                    {
                                        Answer = CommandValueAnswer.GateClosing + " auto gate closing turned off gate is closing right now!"
                                    });
                                }
                            }
                            
                            if (item.CommandValue.Equals(CommandValue.Open) ||
                                item.CommandValue.Equals(CommandValue.ForceOpen))
                            {
                                return Accepted(new AnswerModel()
                                {
                                    Answer = CommandValueAnswer.GateOpening.ToString()
                                });
                            }
                            else if (item.CommandValue.Equals(CommandValue.Close))
                            {
                                return Accepted(new AnswerModel()
                                {
                                    Answer = CommandValueAnswer.GateOpening.ToString()
                                });
                            }
                            else if (item.CommandValue.Equals(CommandValue.ForceClose))
                            {
                                _iDataSingleton.GetSystemStatus().IsGateMoving = true;
                                await _openHab.PostData(_iDataSingleton.GetSettings().GateOpenHabLink, "ON", true);
                                Thread.Sleep(500);
                                await _openHab.PostData(_iDataSingleton.GetSettings().GateOpenHabLink, "ON", true);
                                return Accepted(new AnswerModel()
                                {
                                    Answer = CommandValueAnswer.GateClosing.ToString()
                                });
                            }

                            break;
                        case GateStatus.GateClosed:
                            if (item.CommandValue.Equals(CommandValue.Open) ||
                                item.CommandValue.Equals(CommandValue.ForceOpen))
                            {
                                _iDataSingleton.GetSystemStatus().IsGateMoving = true;
                                await _openHab.PostData(_iDataSingleton.GetSettings().GateOpenHabLink, "ON", true);
                                return Accepted(new AnswerModel()
                                {
                                    Answer = CommandValueAnswer.GateOpening.ToString()
                                });
                            }
                            else if (item.CommandValue.Equals(CommandValue.Close) ||
                                     item.CommandValue.Equals(CommandValue.ForceClose))
                            {
                                return Accepted(new AnswerModel()
                                {
                                    Answer = CommandValueAnswer.AlreadyClosed.ToString()
                                });
                            }

                            break;
                        case GateStatus.GateClosing:
                            if (item.CommandValue.Equals(CommandValue.Close) ||
                                item.CommandValue.Equals(CommandValue.ForceClose))
                            {
                                return Accepted(new AnswerModel()
                                {
                                    Answer = CommandValueAnswer.GateClosing.ToString()
                                });
                            }
                            else if (item.CommandValue.Equals(CommandValue.Open))
                            {
                                return Accepted(new AnswerModel()
                                {
                                    Answer = CommandValueAnswer.GateClosing.ToString()
                                });
                            }
                            else if (item.CommandValue.Equals(CommandValue.ForceOpen))
                            {
                                _iDataSingleton.GetSystemStatus().IsGateMoving = true;
                                await _openHab.PostData(_iDataSingleton.GetSettings().GateOpenHabLink, "ON", true);
                                Thread.Sleep(500);
                                await _openHab.PostData(_iDataSingleton.GetSettings().GateOpenHabLink, "ON", true);
                                return Accepted(new AnswerModel()
                                {
                                    Answer = CommandValueAnswer.GateOpening.ToString()
                                });
                            }
                            break;
                        default:
                            return NotFound();
                    }
                    break;
                case Command.OpenGateAuto:
                    switch (_iDataSingleton.GetSystemStatus().GateStatus)
                    {
                        case GateStatus.GateOpen:
                            return Accepted(new AnswerModel()
                            {
                                Answer = CommandValueAnswer.AlreadyOpen.ToString()
                            });
                        case GateStatus.GateOpening:
                            return Accepted(new AnswerModel()
                            {
                                Answer = CommandValueAnswer.AlreadyOpening.ToString()
                            });
                        case GateStatus.GateClosed:
                            _iDataSingleton.GetSystemStatus().IsGateMoving = true;
                            _iDataSingleton.GetSystemStatus().IsAutoMode = true;
                            await _openHab.PostData(_iDataSingleton.GetSettings().GateOpenHabLink, "ON", true);
                            StartAutoCloseTimer();
                            return Accepted(new AnswerModel()
                            {
                                Answer = CommandValueAnswer.GateOpening.ToString() + " and gate will close automatically!"
                            });
                        case GateStatus.GateClosing:
                            _iDataSingleton.GetSystemStatus().IsGateMoving = true;
                            _iDataSingleton.GetSystemStatus().IsAutoMode = true;
                            await _openHab.PostData(_iDataSingleton.GetSettings().GateOpenHabLink, "ON", true);
                            Thread.Sleep(500);
                            await _openHab.PostData(_iDataSingleton.GetSettings().GateOpenHabLink, "ON", true);
                            StartAutoCloseTimer();
                            return Accepted(new AnswerModel()
                            {
                                Answer = CommandValueAnswer.GateOpening.ToString()
                            });
                        default:
                            return NotFound();
                    }
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
                case Command.OpenDoor when item.CommandValue.Equals(CommandValue.Open) ||
                                           item.CommandValue.Equals(CommandValue.Close):
                case Command.OpenGate when item.CommandValue.Equals(CommandValue.Open) ||
                                           item.CommandValue.Equals(CommandValue.Close) ||
                                           item.CommandValue.Equals(CommandValue.ForceOpen) ||
                                           item.CommandValue.Equals(CommandValue.ForceClose):
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the user exists and has access rights.
        /// </summary>
        /// <param name="auth"></param>
        /// <returns></returns>
        private async Task<ActionResult<AnswerModel>> CheckUser(AuthModel auth)
        {
            if (_context.Users.Any(u => u.Name.Equals(auth.Name)))
            {
                User user = await _context.Users.SingleAsync(u => u.Name.Equals(auth.Name));
                    
                if (user?.Name != null && user.PhoneId != null)
                {
                    if (!user.AccessRights.Equals(AccessRights.Allowed))
                    {
                        return BadRequest(new AnswerModel()
                        {
                            Answer = "User not allowed!"
                        });
                    }
                    
                    if (user.Name.Equals(auth.Name) 
                        && user.PhoneId.Equals(auth.Md5Hash))
                    {
                        user.LastConnection = DateTime.Now;
                        await _context.SaveChangesAsync();

                        return null;
                    }

                    return BadRequest(new AnswerModel()
                    {
                        Answer = "Credentials are wrong!"
                    });
                }
            }
            return BadRequest(new AnswerModel()
            {
                Answer = "Missing Credentials!"
            });
        }

        /// <summary>
        /// Creates a md5Hash from a string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>md5Hash as a string</returns>
        private string GetMd5Hash(string text)
        {
            if((text == null) || (text.Length == 0))
            {
                return string.Empty;
            }
            
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] textToHash = Encoding.Default.GetBytes(text);
            byte[] result = md5.ComputeHash(textToHash); 

            return BitConverter.ToString(result); 
        }

        private void StartAutoCloseTimer()
        {
            _autoCloseTimer.Interval = _autoCloseTimout;
            _autoCloseTimer.Start();
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