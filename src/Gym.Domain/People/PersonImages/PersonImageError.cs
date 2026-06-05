
using Gym.Domain.Common.Result;

namespace Gym.Domain.People.PersonImages;
public static class PersonImageError
{
    public static Error PersonImageUrlRequired => Error.Validation("Person_Image_Url_Required", "Person Image Url Required");
}
