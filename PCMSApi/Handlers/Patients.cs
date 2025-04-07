using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PCMSApi.Services;
using System.Text.Json;
using Swashbuckle.AspNetCore.Annotations;
using PCMSApi.Models;

namespace PCMSApi.Handlers;

/// <summary>
/// Handler for patient-related operations.
/// </summary>
public class Patients(IPatientService _service)
{
    /// <summary>
    /// Retrieves all patients along with their medical history and attachments.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The number of patients per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of patients.</returns>
    [SwaggerOperation(
        Summary = "Get all patients",
        Description = "Returns all patients along with their medical history and attachments."
    )]
    public async Task<IResult> GetAllPatients(int page, int pageSize, CancellationToken cancellationToken)
    {
        var patients = await _service.GetAllPatientsAsync(page, pageSize, cancellationToken);
        return Results.Ok(patients);
    }

    /// <summary>
    /// Retrieves a single patient by their unique identifier.
    /// </summary>
    /// <param name="patientId">The unique identifier of the patient.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The patient details if found, otherwise a 404 status.</returns>
    [SwaggerOperation(
        Summary = "Get a patient by ID",
        Description = "Returns a single patient and their attachments using a UUID patient ID."
    )]
    public async Task<IResult> GetPatientById(Guid patientId, CancellationToken cancellationToken)
    {
        var patientDto = await _service.GetPatientByIdAsync(patientId, cancellationToken);
        return patientDto is not null
            ? Results.Ok(patientDto)
            : Results.NotFound($"Patient with ID {patientId} not found.");
    }

    /// <summary>
    /// Creates a new patient with file uploads.
    /// </summary>
    /// <param name="request">The HTTP request containing the patient data and files.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created patient details.</returns>
    [SwaggerOperation(
        Summary = "Create a patient with file uploads",
        Description = """
        Creates a new patient from a JSON payload and accepts multiple uploaded documents (e.g., MRI, CAT Scan).
        - Use `patient` form field to send JSON
        - Use `files` form field to upload files
        - Use `documentTypes` form field to specify document type for each file
        """
    )]
    [Consumes("multipart/form-data")]
    public async Task<IResult> CreatePatient(HttpRequest request, CancellationToken cancellationToken)
    {
        var form = await request.ReadFormAsync(cancellationToken);
        var patientJson = form["patient"].FirstOrDefault(); // safely get string or null
        var files = form.Files.ToList();

        var documentTypes = form["documentTypes"]
            .Where(dt => !string.IsNullOrWhiteSpace(dt))
            .Select(dt => dt!)
            .ToList();

        if (string.IsNullOrWhiteSpace(patientJson))
            return Results.BadRequest("Missing patient data.");

        if (documentTypes.Count != files.Count)
            return Results.BadRequest("Each uploaded file must have a corresponding document type.");

        PatientDto? patientDto;
        try
        {
            patientDto = JsonSerializer.Deserialize<PatientDto>(patientJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return Results.BadRequest("Invalid JSON for patient data.");
        }

        if (patientDto == null)
            return Results.BadRequest("Could not parse patient data.");

        var createdPatient = await _service.CreatePatient(
            patientDto,
            files,
            documentTypes,
            cancellationToken
        );

        return Results.Created($"/patients/{createdPatient.PatientId}", createdPatient);
    }

    /// <summary>
    /// Updates an existing patient's details.
    /// </summary>
    /// <param name="patientId">The unique identifier of the patient.</param>
    /// <param name="updatedDto">The updated patient data transfer object.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A 200 status if the update was successful, otherwise a 404 status.</returns>
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Update a patient (with optional attachments)",
        Description = "Updates a patient's details and optionally uploads attachments via multipart/form-data."
    )]
    public async Task<IResult> UpdatePatient(HttpRequest request, Guid patientId, CancellationToken cancellationToken)
    {
        var form = await request.ReadFormAsync(cancellationToken);
        var patientJson = form["patient"].FirstOrDefault();
        var files = form.Files.ToList();
        var documentTypes = form["documentTypes"].ToList();

        if (string.IsNullOrWhiteSpace(patientJson))
            return Results.BadRequest("Missing patient data.");

        if (documentTypes.Count != files.Count)
            return Results.BadRequest("Each uploaded file must have a corresponding document type.");

        PatientDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<PatientDto>(patientJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return Results.BadRequest("Invalid JSON for patient.");
        }

        if (dto == null)
            return Results.BadRequest("Could not parse patient data.");

        var updated = await _service.UpdatePatient(patientId, dto, files, documentTypes, cancellationToken);
        return updated ? Results.Ok() : Results.NotFound($"Patient with ID {patientId} not found.");
    }

    /// <summary>
    /// Deletes a patient and all associated attachments.
    /// </summary>
    /// <param name="patientId">The unique identifier of the patient.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A 204 status if the deletion was successful, otherwise a 404 status.</returns>
    [SwaggerOperation(
        Summary = "Delete a patient",
        Description = "Deletes a patient record and all associated attachments."
    )]
    public async Task<IResult> DeletePatient(Guid patientId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeletePatient(patientId, cancellationToken);
        return deleted
            ? Results.NoContent()
            : Results.NotFound($"Patient with ID {patientId} not found.");
    }
}