﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Gauniv.WebServer.Security
{
    internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
            if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
            {
                var requirements = new Dictionary<string, OpenApiSecurityScheme>
                {
                    ["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer", // "bearer" refers to the header name here
                        In = ParameterLocation.Header,
                        BearerFormat = "Json Web Token"
                    }
                };
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes = requirements;
            }
            document.Info = new()
            {
                Title = "PI Bearer scheme",
                Version = "v1",
                Description = "API"
            };
        }
    }
}