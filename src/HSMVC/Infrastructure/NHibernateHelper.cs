using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using HSMVC.Domain;
using NHibernate;
using NHibernate.Cfg;

namespace HSMVC.Infrastructure
{
    public class NHibernateHelper 
    {
        private static ISessionFactory SessionFactory
        {
            get
            {
                return Fluently.Configure()
                    .Database(MsSqlConfiguration.MsSql2012.ConnectionString(c => c.FromConnectionStringWithKey("ConferenceDb")))
                    .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Conference>())
                    .BuildSessionFactory();
            }
        }

        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }
    }
}