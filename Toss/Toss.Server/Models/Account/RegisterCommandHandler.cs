﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Toss.Shared;
using Toss.Server.Models;
using Toss.Shared.Services;
using MediatR;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace Toss.Server.Controllers
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, CommandResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;
        private readonly IEmailSender _emailSender;
        private readonly IUrlHelper urlHelper;
        private readonly IHttpContextAccessor httpContextAccessor;
        public async Task<CommandResult> Handle(RegisterCommand model, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser { UserName = model.Name, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = urlHelper.EmailConfirmationLink(user.Id, code, httpContextAccessor.HttpContext.Request.Scheme);
                await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                //await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("User created a new account with password.");
                return CommandResult.Success();
            }
            return new CommandResult("UserName", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}