using System;

namespace HSMVC.Features.Conference.Commands
{
    public class ConferenceAddCommand
    {
        public string Name { get; set; }
        public string Hashtag { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Cost { get; set; }
        public int? AttendeeCount { get; set; }
    }
}