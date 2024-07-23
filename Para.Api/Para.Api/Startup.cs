using System.Reflection;
using System.Text.Json.Serialization;
using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Para.Api.Middleware;
using Para.Api.Service;
using Para.Bussiness;
using Para.Bussiness.Cqrs;
using Para.Data.Context;
using Para.Data.UnitOfWork;
using Para.Validation;
using Autofac;
using Para.Base.Response;
using Para.Bussiness.Query;
using Para.Schema;

namespace Para.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Para.Api", Version = "v1" });
            });


            var connectionStringSql = Configuration.GetConnectionString("MsSqlConnection");
            services.AddDbContext<ParaDbContext>(options => options.UseSqlServer(connectionStringSql));


        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(CustomerValidator).Assembly,
                                          typeof(CustomerDetailValidator).Assembly,
                                          typeof(CustomerAddressValidator).Assembly,
                                          typeof(CustomerPhoneValidator).Assembly)
                .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
                .AsImplementedInterfaces();

  
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();


            builder.RegisterAssemblyTypes(typeof(CreateCustomerCommand).GetTypeInfo().Assembly,
                                          typeof(CreateCustomerDetailCommand).GetTypeInfo().Assembly,
                                          typeof(CreateCustomerAddressCommand).GetTypeInfo().Assembly,
                                          typeof(CreateCustomerPhoneCommand).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(IRequestHandler<,>));

            builder.RegisterAssemblyTypes(typeof(CustomerQueryHandler).GetTypeInfo().Assembly,
                                          typeof(CustomerDetailQueryHandler).GetTypeInfo().Assembly,
                                          typeof(CustomerAddressQueryHandler).GetTypeInfo().Assembly,
                                          typeof(CustomerPhoneQueryHandler).GetTypeInfo().Assembly)
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly)
                .AsImplementedInterfaces();


            builder.Register(context => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperConfig>();
            })).AsSelf().SingleInstance();

            builder.Register(c =>
            {
                var context = c.Resolve<IComponentContext>();
                var config = context.Resolve<MapperConfiguration>();
                return config.CreateMapper(context.Resolve);
            }).As<IMapper>().InstancePerLifetimeScope();

            builder.Register<ServiceFactory>(context =>
            {
                var componentContext = context.Resolve<IComponentContext>();
                return t => componentContext.TryResolve(t, out var o) ? o : default!;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Para.Api v1"));
            }
            app.UseMiddleware<LoggingMiddleware>();
            app.UseMiddleware<HeartbeatMiddleware>();
            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}