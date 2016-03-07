using System;
using HSMVC.DataAccess;
using StructureMap;

namespace HSMVC.Features.Conference.Validation
{
    public class ConferenceValidatorHelper
    {
        private readonly IConferenceRepository _conferenceRepository;

        public ConferenceValidatorHelper(IConferenceRepository conferenceRepository)
        {
            _conferenceRepository = conferenceRepository;
        }

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

        public bool DoesConferenceNameAlreadyExist(Guid? id, string nameToCheck)
        {
            if (string.IsNullOrEmpty(nameToCheck))
                return false;

            var conference = _conferenceRepository.FindByName(nameToCheck);

            if (conference == null) return false;

            var isEditingConference = id == conference.Id;

            return !isEditingConference;
        }
    }
}