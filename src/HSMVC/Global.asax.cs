using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using FluentValidation;
using FluentValidation.Mvc;
using HSMVC.DependencyResolution;
using HSMVC.Infrastructure.AutoMapper;
using HSMVC.Infrastructure.FluentValidation;
using StructureMap;

namespace HSMVC
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var container = StructuremapMvc.Start();
            Mapper.Initialize(cfg => cfg.AddProfile<AutoMapperProfile>());
            FluentValidationModelValidatorProvider.Configure(provider =>
            {
                provider.ValidatorFactory = new StructureMapValidatorFactory(container);
            });
        }
    }
}
