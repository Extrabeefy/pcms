namespace PCMSApi.Endpoints
{
    /// <summary>
    /// Defines the endpoints for health check operations.
    /// </summary>
    public static class HealthEndpoints
    {
        /// <summary>
        /// Maps the health check endpoints to the application.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/health", () => Results.Ok("Healthy"))
                .WithName("GetHealthStatus")
                .WithDescription("Returns the health status of the application.")
                .Produces(StatusCodes.Status200OK);
        }
    }
}