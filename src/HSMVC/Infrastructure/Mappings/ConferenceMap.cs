using FluentNHibernate.Mapping;
using HSMVC.Domain;

namespace HSMVC.Infrastructure.Mappings
{
    public class ConferenceMap : ClassMap<Conference>
    {
        public ConferenceMap()
        {
            Id(x => x.Id);
            Map(x => x.Name);
            Map(x => x.HashTag);
            Map(x => x.StartDate);
            Map(x => x.EndDate);
            Map(x => x.Cost);
            Map(x => x.AttendeeCount).Column("Attendees");
        }
    }
}