﻿namespace Plexus.Interop.Metamodel.Json
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Plexus.Interop.Metamodel.Json.Internal;

    public static class JsonRegistry
    {
        public static IRegistry LoadRegistry(string fileName)
        {
            using (var stream = File.Open(fileName, FileMode.Open))
            {
                return LoadRegistry(stream);
            }
        }

        public static IRegistry LoadRegistry(Stream stream)
        {
            var dto = RegistryDto.LoadFromStream(stream);
            var messageIds =
                from svcDto in dto.Services
                from metDto in svcDto.Methods
                from mid in new[] { metDto.InputMessageId, metDto.OutputMessageId }
                select mid;
            var messages = messageIds.Distinct().ToDictionary(x => x, x => (IMessage)new Message { Id = x });
            var services = dto.Services.Select(x => Convert(messages, x)).ToDictionary(x => x.Id, x => x);
            var applications = dto.Applications.Select(x => Convert(services, x)).ToDictionary(x => x.Id, x => x);
            return new Registry
            {
                Messages = messages,
                Services = services,
                Applications = applications
            };
        }

        private static IService Convert(IReadOnlyDictionary<string, IMessage> messages, ServiceDto svcDto)
        {
            var service = new Service { Id = svcDto.Id };
            service.Methods = svcDto.Methods.Select(x =>
                (IMethod)new Method
                {
                    Name = x.Name,
                    InputMessage = messages[x.InputMessageId],
                    OutputMessage = messages[x.OutputMessageId],
                    Service = service,
                    Type = Convert(x.Type)
                })
                .ToDictionary(x => x.Name, x => x);
            return service;
        }

        private static IApplication Convert(
            IReadOnlyDictionary<string, IService> services,
            ApplicationDto appDto)
        {
            var app = new Application { Id = appDto.Id };
            app.ConsumedServices = appDto.ConsumedServices.Select(x => Convert(app, services[x.ServiceId], x)).ToList();
            app.ProvidedServices = appDto.ProvidedServices.Select(x => Convert(app, services[x.ServiceId], x)).ToList();
            return app;
        }

        private static IConsumedService Convert(
            IApplication application,
            IService service,
            ConsumedServiceDto dto)
        {
            var cs = new ConsumedService
            {
                Alias = ConvertMaybe(dto.Alias),
                Service = service,
                Application = application,
                From = ConvertMatchPatterns(dto.From)
            };
            cs.Methods = dto.Methods
                .Select(x => (IConsumedMethod)new ConsumedMethod { Method = service.Methods[x], ConsumedService = cs })
                .ToDictionary(x => x.Method.Name, x => x);
            return cs;
        }

        private static IMatchPattern ConvertMatchPatterns(IReadOnlyCollection<string> dto)
        {
            return dto.Count == 0
                ? (IMatchPattern)AnyMatchPattern.Instance
                : new CompositeMatchPattern(dto.Select(ConvertMatchPattern));
        }

        private static IMatchPattern ConvertMatchPattern(string str)
        {
            return str.EndsWith("*")
                ? new MatchPattern(MatchType.StartsWith, str.Substring(0, str.Length - 1))
                : new MatchPattern(MatchType.Exact, str);
        }

        private static IProvidedService Convert(
            IApplication application,
            IService service,
            ProvidedServiceDto dto)
        {
            var ps = new ProvidedService
            {
                Alias = ConvertMaybe(dto.Alias),
                Service = service,
                Title = ConvertMaybe(dto.Title),
                Application = application,
                To = ConvertMatchPatterns(dto.To)
            };
            ps.Methods = dto.Methods
                .Select(x => (IProvidedMethod)new ProvidedMethod { Method = service.Methods[x.Name], ProvidedService = ps, Title = ConvertMaybe(x.Title) })
                .ToDictionary(x => x.Method.Name, x => x);
            return ps;
        }

        private static MethodType Convert(MethodTypeDto dto)
        {
            switch (dto)
            {
                case MethodTypeDto.Unary:
                    return MethodType.Unary;
                case MethodTypeDto.ServerStreaming:
                    return MethodType.ServerStreaming;
                case MethodTypeDto.ClientStreaming:
                    return MethodType.ClientStreaming;
                case MethodTypeDto.DuplexStreaming:
                    return MethodType.DuplexStreaming;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dto), dto, null);
            }
        }

        private static Maybe<string> ConvertMaybe(string s)
        {
            return string.IsNullOrEmpty(s) ? Maybe<string>.Nothing : new Maybe<string>(s);
        }
    }
}
