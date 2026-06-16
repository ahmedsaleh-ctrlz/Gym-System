using Gym.Application.Features.Coaches.Dtos;
using Gym.Domain.Coaches;

namespace Gym.Application.Features.Coaches.Mappers
{
    public static class CoachMapper
    {
        public static CoachResponse ToDto(this Coach Coach)
        {
            ArgumentNullException.ThrowIfNull(Coach);

            return new CoachResponse
            {
                CoachId = Coach.Id,
                FirstName = Coach.Person.FirstName,
                LastName = Coach.Person.LastName,
                DateOfBirth = Coach.Person.DateOfBirth,
                PhoneNumber = Coach.Person.PhoneNumber,
                ImageUrl = Coach.Person.Image.ImageUrl,
                HireDate = Coach.HireDate
            };
        }
    }
}