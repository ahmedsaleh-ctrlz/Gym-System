using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Features.Identity.Dtos;

public class TokenResponse
{
    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

}
