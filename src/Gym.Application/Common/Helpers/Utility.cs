using System;
using System.Collections.Generic;
using System.Text;

using Gym.Domain.Common.Result;

using Microsoft.Extensions.Logging;

namespace Gym.Application.Common.Helpers
{
    public static class Utility
    {
        public async static Task<Result<Deleted>> DeleteImage(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                return Error.NotFound("Image not found");
            }

            try
            {
                File.Delete(imagePath);
                return Result.Deleted;
            }
            catch (IOException)
            {
                return Error.Failure("Delete failed");
            }
        }

        public static string MaskEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return string.Empty;
            }

            var atIndex = email.IndexOf('@');

            if (atIndex <= 1)
            {
                return email;
            }

            var username = email[..atIndex];
            var domain = email[atIndex..];

            if (username.Length <= 2)
            {
                return $"{username[0]}*{domain}";
            }

            return $"{username[0]}{new string('*', username.Length - 2)}{username[^1]}{domain}";
        }
    }
}