using Gym.Domain.Common.Result;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Common.Helpers
{
    public static class Utility
    {
        public async static Task<Result<Deleted>> DeleteImage(string imagePath)
        {
            if (!File.Exists(imagePath))
                return Error.NotFound("Image not found");

            try
            {
                File.Delete(imagePath);
                return Result.Deleted;
            }
            catch (IOException ex)
            {
                return Error.Failure("Delete failed");
            }
        }
    }
}
