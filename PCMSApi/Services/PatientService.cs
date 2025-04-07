using Amazon.S3.Model;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PCMSApi.Data;
using PCMSApi.Models;
using System.Text.Json;

namespace PCMSApi.Services;

public class PatientService(AppDb db, IS3Service s3Service, IMapper mapper) : IPatientService
{
    /// <summary>
    /// Retrieves a paginated list of patients.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The number of patients per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of PatientDto objects.</returns>
    public async Task<List<PatientDto>> GetAllPatientsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = db.Patients
            .Include(p => p.Attachments)
            .Include(p => p.MedicalHistory)
            .OrderBy(p => p.Name); // Optional sort for consistent pagination

        var patients = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = mapper.Map<List<PatientDto>>(patients);

        foreach (var dto in dtos)
        {
            var patientEntity = patients.FirstOrDefault(p => p.PatientUid == dto.PatientId);
            if (patientEntity == null) continue;

            foreach (var attachmentDto in dto.Attachments)
            {
                var matchingEntity = patientEntity.Attachments
                    .FirstOrDefault(a => a.AttachmentId == attachmentDto.AttachmentId);

                if (!string.IsNullOrEmpty(matchingEntity?.S3Key))
                {
                    attachmentDto.Url = s3Service.GeneratePresignedUrl(matchingEntity.S3Key, TimeSpan.FromHours(1));
                }
            }
        }

        return dtos;
    }

    /// <summary>
    /// Retrieves a patient by their unique identifier.
    /// </summary>
    /// <param name="patientUid">The unique identifier of the patient.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A PatientDto object if found, otherwise null.</returns>
    public async Task<PatientDto?> GetPatientByIdAsync(Guid patientUid, CancellationToken cancellationToken)
    {
        var patient = await db.Patients
            .Include(p => p.Attachments)
            .Include(p => p.MedicalHistory)
            .FirstOrDefaultAsync(p => p.PatientUid == patientUid, cancellationToken);

        if (patient is null)
            return null;

        var dto = mapper.Map<PatientDto>(patient);

        // Generate presigned URLs
        foreach (var attachmentDto in dto.Attachments)
        {
            var s3Key = patient.Attachments
                .FirstOrDefault(a => a.AttachmentId == attachmentDto.AttachmentId)?.S3Key;

            if (!string.IsNullOrEmpty(s3Key))
            {
                attachmentDto.Url = s3Service.GeneratePresignedUrl(s3Key, TimeSpan.FromHours(1));
            }
        }

        return dto;
    }

    /// <summary>
    /// Creates a new patient.
    /// </summary>
    /// <param name="dto">The patient data transfer object.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created PatientDto object.</returns>
    public async Task<PatientDto> CreatePatientAsync(PatientDto dto, CancellationToken cancellationToken)
    {
        var patient = mapper.Map<Patient>(dto);
        patient.PatientUid = Guid.NewGuid();

        db.Patients.Add(patient);
        await db.SaveChangesAsync(cancellationToken);

        return mapper.Map<PatientDto>(patient);
    }

    /// <summary>
    /// Creates a new patient with associated files.
    /// </summary>
    /// <param name="dto">The patient data transfer object.</param>
    /// <param name="files">The list of files to upload.</param>
    /// <param name="documentTypes">The list of document types corresponding to the files.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created PatientDto object.</returns>
    public async Task<PatientDto> CreatePatientWithFilesAsync(
        PatientDto dto,
        List<IFormFile> files,
        List<string> documentTypes,
        CancellationToken ct)
    {
        var patientUid = Guid.NewGuid();

        var patient = new Patient
        {
            PatientUid = patientUid,
            Name = dto.Name,
            Age = dto.Age,
            ContactPhone = dto.ContactPhone,
            ContactEmail = dto.ContactEmail,
            ContactAddress = dto.ContactAddress,
            MedicalHistory = dto.MedicalHistory
        };

        db.Patients.Add(patient);
        await db.SaveChangesAsync(ct); // generates internal Id for FK usage

        var attachments = new List<AttachmentDto>();
        var allowedTypes = new[] { "MRI", "CAT_SCAN", "DOCTOR_REPORT" };

        for (int i = 0; i < files.Count; i++)
        {
            var file = files[i];

            var documentType = (documentTypes.Count > i) ? documentTypes[i] : null;
            if (!string.IsNullOrWhiteSpace(documentType) && !allowedTypes.Contains(documentType))
            {
                throw new ArgumentException(
                    $"Invalid document type '{documentType}' for file '{file.FileName}'. Allowed values: {string.Join(", ", allowedTypes)}");
            }

            var validatedDocumentType = string.IsNullOrWhiteSpace(documentType) ? "UNKNOWN" : documentType;

            var attachmentId = Guid.NewGuid();
            var s3Key = $"patients/{patientUid}/{documentType}/{attachmentId}/{file.FileName}";

            await s3Service.UploadAsync(file, s3Key, ct);

            var attachment = new Attachment
            {
                AttachmentId = attachmentId,
                PatientId = patient.Id, // internal FK
                FileName = file.FileName,
                DocumentType = validatedDocumentType,
                S3Key = s3Key,
                UploadedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["original_filename"] = file.FileName,
                    ["document_type"] = validatedDocumentType,
                    ["uploaded_by"] = "system"
                }
            };

            db.Attachments.Add(attachment);

            var url = s3Service.GeneratePresignedUrl(s3Key, TimeSpan.FromHours(1));

            attachments.Add(new AttachmentDto
            {
                AttachmentId = attachment.AttachmentId,
                FileName = attachment.FileName,
                DocumentType = attachment.DocumentType,
                UploadedAt = attachment.UploadedAt,
                Url = url
            });
        }

        await db.SaveChangesAsync(ct);

        var mapped = mapper.Map<PatientDto>(patient);
        mapped.Attachments = attachments;
        return mapped;
    }

    /// <summary>
    /// Updates an existing patient.
    /// </summary>
    /// <param name="patientUid">The unique identifier of the patient.</param>
    /// <param name="dto">The patient data transfer object.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the update was successful, otherwise false.</returns>
    public async Task<bool> UpdatePatientAsync(Guid patientUid, PatientDto dto, CancellationToken cancellationToken)
    {
        var existing = await db.Patients
            .Include(p => p.MedicalHistory)
            .FirstOrDefaultAsync(p => p.PatientUid == patientUid, cancellationToken);

        if (existing is null) return false;

        // Update basic fields
        existing.Name = dto.Name;
        existing.Age = dto.Age;
        existing.ContactPhone = dto.ContactPhone;
        existing.ContactEmail = dto.ContactEmail;
        existing.ContactAddress = dto.ContactAddress;

        // Update medical history
        db.Conditions.RemoveRange(existing.MedicalHistory);
        existing.MedicalHistory = dto.MedicalHistory.Select(m => new MedicalCondition
        {
            PatientId = existing.Id,
            Condition = m.Condition,
            Notes = m.Notes,
            Since = m.Since,
            Frequency = m.Frequency,
            History = m.History,
            Status = m.Status
        }).ToList();

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Updates an existing patient with associated files.
    /// </summary>
    /// <param name="patientUid">The unique identifier of the patient.</param>
    /// <param name="dto">The patient data transfer object.</param>
    /// <param name="files">The list of files to upload.</param>
    /// <param name="documentTypes">The list of document types corresponding to the files.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the update was successful, otherwise false.</returns>
    public async Task<bool> UpdatePatientWithFilesAsync(Guid patientUid, PatientDto dto, List<IFormFile> files, List<string> documentTypes, CancellationToken cancellationToken)
    {
        var patient = await db.Patients
            .Include(p => p.MedicalHistory)
            .Include(p => p.Attachments)
            .FirstOrDefaultAsync(p => p.PatientUid == patientUid, cancellationToken);

        if (patient is null) return false;

        // Update base fields
        patient.Name = dto.Name;
        patient.Age = dto.Age;
        patient.ContactPhone = dto.ContactPhone;
        patient.ContactEmail = dto.ContactEmail;
        patient.ContactAddress = dto.ContactAddress;

        // Replace medical history
        db.Conditions.RemoveRange(patient.MedicalHistory);
        patient.MedicalHistory = dto.MedicalHistory.Select(m => new MedicalCondition
        {
            PatientId = patient.Id,
            Condition = m.Condition,
            Notes = m.Notes,
            Since = m.Since,
            Frequency = m.Frequency,
            History = m.History,
            Status = m.Status
        }).ToList();

        // Handle new attachments
        for (int i = 0; i < files.Count; i++)
        {
            var file = files[i];
            var documentType = (documentTypes.Count > i) ? documentTypes[i] : "UNKNOWN";

            var attachmentId = Guid.NewGuid();
            var s3Key = $"patients/{patient.PatientUid}/{documentType}/{attachmentId}/{file.FileName}";

            await s3Service.UploadAsync(file, s3Key, cancellationToken);

            var attachment = new Attachment
            {
                AttachmentId = attachmentId,
                PatientId = patient.Id,
                FileName = file.FileName,
                DocumentType = documentType,
                S3Key = s3Key,
                UploadedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["original_filename"] = file.FileName,
                    ["document_type"] = documentType,
                    ["uploaded_by"] = "system"
                }
            };

            db.Attachments.Add(attachment);
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Deletes a patient by their unique identifier.
    /// </summary>
    /// <param name="patientUid">The unique identifier of the patient.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the deletion was successful, otherwise false.</returns>
    public async Task<bool> DeletePatientAsync(Guid patientUid, CancellationToken cancellationToken)
    {
        var patient = await db.Patients
            .Include(p => p.Attachments)
            .FirstOrDefaultAsync(p => p.PatientUid == patientUid, cancellationToken);

        if (patient is null) return false;

        foreach (var attachment in patient.Attachments)
        {
            try
            {
                await s3Service.DeleteAsync(attachment.S3Key, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting S3 file {attachment.S3Key}: {ex.Message}");
            }
        }

        db.Patients.Remove(patient);
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Deletes an attachment by its unique identifier.
    /// </summary>
    /// <param name="patientUid">The unique identifier of the patient.</param>
    /// <param name="attachmentId">The unique identifier of the attachment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the deletion was successful, otherwise false.</returns>
    public async Task<bool> DeleteAttachmentAsync(Guid patientUid, Guid attachmentId, CancellationToken cancellationToken)
    {
        var patient = await db.Patients
            .Include(p => p.Attachments)
            .FirstOrDefaultAsync(p => p.PatientUid == patientUid, cancellationToken);

        if (patient is null) return false;

        var attachment = patient.Attachments.FirstOrDefault(a => a.AttachmentId == attachmentId);
        if (attachment is null) return false;

        db.Attachments.Remove(attachment);

        // Optional: remove the file from S3 too
        await s3Service.DeleteAsync(attachment.S3Key, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}