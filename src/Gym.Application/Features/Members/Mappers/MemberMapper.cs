using Gym.Application.Features.Members.Dtos;
using Gym.Domain.Common;
using Gym.Domain.Members;

namespace Gym.Application.Features.Members.Mappers
{
    public static class MemberMapper
    {
        public static MemberResponse ToDto(this Member member)
        {
            ArgumentNullException.ThrowIfNull(member);

            return new MemberResponse
            {
                MemberId = member.Id,

                FirstName = member.Person.FirstName,
                LastName = member.Person.LastName,
                DateOfBirth = member.Person.DateOfBirth,
                PhoneNumber = member.Person.PhoneNumber,
                ImageUrl = member.Person.Image.ImageUrl,
                JoinDate = member.JoinDate,
                Notes = member.Notes
            };
        }

        public static List<ActiveMemberResponse> ToActiveMemberDtos(
                    this IEnumerable<Member> members)
        {
            return members.Select(m => new ActiveMemberResponse
            {
                MemberId = m.Id,
                FirstName = m.Person.FirstName,
                LastName = m.Person.LastName,
                PhoneNumber = m.Person.PhoneNumber,
                ImageUrl = m.Person.Image.ImageUrl
            }).ToList();
        }
    }
}