using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Gym.Application.Features.Identity.Dtos;

public sealed record AppUserDto(string UserId,int? PersonId,string Email, IList<string> Roles, IList<Claim> Claims);
