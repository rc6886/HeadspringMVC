using System;
using HSMVC.DataAccess;
using StructureMap;

namespace HSMVC.Features.Conference.Validation
{
    public class ConferenceValidatorHelper
    {
        public static string RequiredMessage(string propertyName)
        {
            return $"{propertyName} is a required field.";
        }

        public static string NotAValidDateMessage(string propertyName)
        {
            return $"{propertyName} is not a valid date.";
        }

        public static bool IsAValidDate(DateTime? date)
        {
            return !date.Equals(default(DateTime));
        }

        public static bool DoesConferenceNameAlreadyExist(Guid? id, string nameToCheck)
        {
            if (string.IsNullOrEmpty(nameToCheck))
                return false;

            var conference = ObjectFactory.GetInstance<IConferenceRepository>()
                .FindByName(nameToCheck);

            if (conference == null) return false;

            var isEditingConference = id == conference.Id;

            return !isEditingConference;
        }
    }
}