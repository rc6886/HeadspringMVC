using System.Collections.Generic;

namespace HSMVC.Features.Conference.Commands
{
    public class ConferenceBulkEditCommand
    {
        public IList<ConferenceEditCommand> Commands { get; set; } 
    }
}