using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PCMSApi.Services;
using PCMSApi.Handlers;
using PCMSApi.Models;

namespace PCMSApi.Endpoints;

/// <summary>
/// Defines the endpoints for patient-related operations.
/// </summary>
public static class PatientEndpoints
{
    /// <summary>
    /// Maps the patient endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapPatientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/patients")
                       .WithTags("Patients")
                       .RequireAuthorization();

        group.MapGet("/", async (int page, int pageSize, IPatientService service, CancellationToken ct) =>
                await new Patients(service).GetAllPatients(page, pageSize, ct))
            .WithName("GetAllPatients")
            .WithDescription("Returns paginated patients with their attachments.")
            .Produces<IEnumerable<PatientDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IPatientService service, CancellationToken ct) =>
                await new Patients(service).GetPatientById(id, ct))
            .WithName("GetPatientById")
            .WithDescription("Get a specific patient by ID, including medical history and attachments.")
            .Produces<PatientDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (HttpRequest request, IPatientService service, CancellationToken ct) =>
                await new Patients(service).CreatePatient(request, ct))
            .WithName("CreatePatient")
            .WithDescription("Create a new patient and upload multiple documents in a multipart/form-data request.")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<PatientDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (HttpRequest request, Guid id, Patients handler, CancellationToken ct) =>
                await handler.UpdatePatient(request, id, ct))
            .WithName("UpdatePatient")
            .WithDescription("Update a patient and optionally upload new attachments.")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", async (Guid id, IPatientService service, CancellationToken ct) =>
                await new Patients(service).DeletePatient(id, ct))
            .WithName("DeletePatient")
            .WithDescription("Deletes a patient and all associated attachments.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/attachments/{attachmentId:guid}", async (
                Guid id,
                Guid attachmentId,
                IPatientService service,
                CancellationToken ct) =>
        {
            var result = await service.DeleteAttachment(id, attachmentId, ct);
            return result
                ? Results.Ok()
                : Results.NotFound("Attachment or Patient not found.");
        })
            .WithName("DeleteAttachment")
            .WithDescription("Deletes a specific attachment for a patient.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}