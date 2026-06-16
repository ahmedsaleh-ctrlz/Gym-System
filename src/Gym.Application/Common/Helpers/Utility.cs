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

        public static string MaskEmail(string email)
        {
            int atIndex = email.IndexOf('@');
            if (atIndex <= 1)
            {
                return $"****{email.AsSpan(atIndex)}";
            }

            return email[0] + "****" + email[atIndex - 1] + email[atIndex..];
        }
    }
}